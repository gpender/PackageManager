using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using _3S.CoDeSys.Core.Components;
using Ionic.Zip;
using PackageManager.Serialization;
using PackageManager.Views;

namespace PackageManager.Model
{
    public class CodesysPackageManager
    {
        string commonPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Parker Package Manager\";
        string tmpPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Parker Package Manager\temp\";
        string packageFile = null;
        string packageManifest = null;
        Dictionary<string, Version> AssemblyNameRegister = new Dictionary<string, Version>();
        Dictionary<string, string> AssemblyNameVersionRegister = new Dictionary<string, string>();
        ObservableCollection<DependencyConflict> dependencyConflicts = new ObservableCollection<DependencyConflict>();
        Package package = null;

        public ObservableCollection<DependencyConflict> DependencyConflicts
        {
            get { return dependencyConflicts; }
        }

        public Package Package
        {
            get { return package; }
            set { package = value; }
        }

        public string IconPath
        {
            get { return tmpPath + @"\" + package.General.Icon.Replace(@"/",@"\"); }
        }

        public CodesysPackageManager()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                commonPath = ApplicationDeployment.CurrentDeployment.DataDirectory + @"\PPM\";
                tmpPath = ApplicationDeployment.CurrentDeployment.DataDirectory + @"\PPM\tmp\";
            }
            packageFile = tmpPath + @"\parker.package";
            packageManifest = tmpPath + @"\package.manifest";
            SetupTempFolders();
            ReadPackage(true);
        }

        public void ReadPackage(bool newEmptyPackage = false)
        {
            SetAttributesToNormal(commonPath);
            DependencyConflicts.Clear();
            AssemblyNameRegister.Clear();
            if (newEmptyPackage)
            {
                using (Stream stream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("PackageManager.Resources.Files.emptyPackage.manifest"))
                {
                    if (stream2 != null)
                    {
                        using (FileStream destination = new FileStream(tmpPath + @"\package.manifest", FileMode.Create, FileAccess.ReadWrite))
                        {
                            stream2.CopyTo(destination);
                        }
                    }
                }
                using (Stream stream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("PackageManager.Resources.Images.parker.ico"))
                {
                    if (stream2 != null)
                    {
                        using (FileStream destination = new FileStream(tmpPath + @"\Icon\parker.ico", FileMode.Create, FileAccess.ReadWrite))
                        {
                            stream2.CopyTo(destination);
                        }
                    }
                }
                PopulatePackageObject();
                CreatePackageGuid(false);
            }
            else
            {
                if (ReadExistingPackageIntoTempLocation())
                {
                    PopulatePackageObject();
                }
            }
            SetAttributesToNormal(commonPath);
        }

        void SetAttributesToNormal(string path)
        {
            foreach (string f in Directory.GetFiles(path))
            {
                FileInfo fileInfo = new FileInfo(f);
                fileInfo.Attributes = FileAttributes.Normal;
            }
            foreach (var d in Directory.GetDirectories(path))
            {
                SetAttributesToNormal(d);
            }
        }

        internal void CreatePackage()
        {
            if (File.Exists(packageManifest))
            {
                File.Delete(packageManifest);
            }
            //if (package.Strings != null)
            //{
            //    foreach (var s in package.Strings)
            //    {
            //        if (s.Id == "GeneralName")
            //        {
            //            s.Neutral = "PDD " + package.General.Version + " Package. For CoDeSys " + targetCodesysVersion;
            //        }
            //    }
            //}
            package.General.Icon = package.General.Icon == string.Empty ? null : package.General.Icon;
            package.General.HTML = package.General.HTML == string.Empty ? null : package.General.HTML;
            XElement xelement = package.Serialize();
            xelement.Save(packageManifest);
            ZipUpPackageAndFiles();

        }

        private void ZipUpPackageAndFiles()
        {
            if (File.Exists(packageFile))
            {
                File.Delete(packageFile);
            }
            string newPackageFile = null;
            System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog();
            saveDialog.FileName = "parker.package";
            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                newPackageFile = saveDialog.FileName;
            }
            if (newPackageFile != null)
            {
                if (File.Exists(newPackageFile))
                {
                    File.Delete(newPackageFile);
                }
                using (ZipFile zip = new ZipFile(newPackageFile))
                {
                    zip.AddDirectory(tmpPath);
                    zip.Save();
                }
            }
        }

        private void PopulatePackageObject(string starterManifest = null)
        {
            packageManifest = tmpPath + @"\package.manifest";
            starterManifest = starterManifest == null ? packageManifest : starterManifest;
            package = SerializationExtensions.Deserialize<Package>(XElement.Load(starterManifest));
            if (package.Components.Component.Items.PlugIn != null)
            {
                foreach (var plugin in package.Components.Component.Items.PlugIn)
                {
                    string filePath = tmpPath + @"\" + plugin.Path;
                    Guid pluginGuid = Guid.Empty;
                    List<AssemblyName> assys = GetPluginGuid(filePath, out pluginGuid);
                    AddAssembliesToRegister(plugin.Path, pluginGuid, assys);
                    plugin.PlugInGuid = pluginGuid;
                    plugin.Version = GetFileVersion(filePath);
                }
            }

            if (package.Components.Component.Items.File != null)
            {
                foreach (var f in package.Components.Component.Items.File)
                {
                    string filePath = tmpPath + @"\" + f.Path;
                    f.IsInterface = f.Path.ToLowerInvariant().EndsWith(".dll");
                    f.Version = GetFileVersion(filePath);
                }
            }
            if (package.Components.Component.Items.Folder != null)
            {
                foreach (var f in package.Components.Component.Items.Folder)
                {
                    f.FolderHierarchy.Add(new UserDirectory(tmpPath + f.Path));
                }
            }

            if (package.Components.Component.Items.DeviceDescription != null)
            {
                foreach (var f in package.Components.Component.Items.DeviceDescription)
                {
                    string filePath = tmpPath + @"\" + f.Path;
                    DeviceDescription devDesc = SerializationExtensions.Deserialize<DeviceDescription>(XElement.Load(filePath));
                    f.Version = devDesc.Device.DeviceIdentification.Version;
                    f.DeviceId = devDesc.Device.DeviceIdentification.Id;
                    f.DeviceType = devDesc.Device.DeviceIdentification.Type.ToString();
                }
            }
        }

        private void AddAssembliesToRegister(string parentAssy, Guid parentAssyGuid, List<AssemblyName> assys)
        {
            foreach (var a in assys)
            {
                string pluginDllName = parentAssy;
                if (pluginDllName.Contains('\\'))
                {
                    string[] tmp = parentAssy.Split(new char[] { '\\' });
                    pluginDllName = tmp[tmp.Length - 1];
                }
                if (AssemblyNameRegister.ContainsKey(a.Name))
                {
                    if (!AssemblyNameRegister[a.Name].Equals(a.Version))
                    {
                        DependencyConflicts.Add(new DependencyConflict() { ParentAssy = pluginDllName, ParentAssyGuid = parentAssyGuid, ConflictingVersion = a.Version, DependencyName = a.Name, Version = AssemblyNameRegister[a.Name] });
                    }
                }
                else
                {
                    AssemblyNameRegister.Add(a.Name, a.Version);
                }
                if (!AssemblyNameVersionRegister.ContainsKey(a.Name + "_" + a.Version.ToString()))
                {
                    AssemblyNameVersionRegister.Add(a.Name + "_" + a.Version.ToString(), pluginDllName);
                }
                else
                {
                    AssemblyNameVersionRegister[a.Name + "_" + a.Version.ToString()] += ";" + pluginDllName;
                }
            }
        }

        private void RemoveAssembliesFromRegisterUsingParentGuid(Guid parentAssyGuid)
        {
            for (int i = DependencyConflicts.Count - 1; i >= 0; i--)
            {
                if (DependencyConflicts.ElementAt(i).ParentAssyGuid == parentAssyGuid)
                {
                    dependencyConflicts.RemoveAt(i);
                }
            }
        }

        internal void CreatePackageGuid(bool confirm=true)
        {
            if (confirm)
            {
                ConfirmDialogCW confirmDialog = new ConfirmDialogCW("Create New GUID", "Are you sure you need a new GUID for this package?");
                confirmDialog.ShowDialog();
                if (!confirmDialog.DialogResult.Value)
                {
                    return;
                }
            }
            Package.General.Id = Guid.NewGuid().ToString();
        }

        private bool ReadExistingPackageIntoTempLocation()
        {
            string existingZipFile = null;
            try
            {
                SetupTempFolders();
                System.Windows.Forms.OpenFileDialog loadDialog = new System.Windows.Forms.OpenFileDialog();
                if (loadDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    existingZipFile = loadDialog.FileName;
                }
                if (existingZipFile != null)
                {
                    bool header = true;
                    using (ZipFile zip = ZipFile.Read(existingZipFile))
                    {
                        foreach (ZipEntry e in zip)
                        {
                            if (header)
                            {
                                System.Console.WriteLine("Zipfile: {0}", zip.Name);
                                if ((zip.Comment != null) && (zip.Comment != ""))
                                    System.Console.WriteLine("Comment: {0}", zip.Comment);
                                System.Console.WriteLine("\n{1,-22} {2,8}  {3,5}   {4,8}  {5,3} {0}",
                                                         "Filename", "Modified", "Size", "Ratio", "Packed", "pw?");
                                System.Console.WriteLine(new System.String('-', 72));
                                header = false;
                            }
                            System.Console.WriteLine("{1,-22} {2,8} {3,5:F0}%   {4,8}  {5,3} {0}",
                                                     e.FileName,
                                                     e.LastModified.ToString("yyyy-MM-dd HH:mm:ss"),
                                                     e.UncompressedSize,
                                                     e.CompressionRatio,
                                                     e.CompressedSize,
                                                     (e.UsesEncryption) ? "Y" : "N");
                            e.Extract(tmpPath, ExtractExistingFileAction.OverwriteSilently);
                            e.Attributes = FileAttributes.Normal;
                        }
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(existingZipFile + " " + e.Message);
            }
            return false;
        }

        private void SetupTempFolders()
        {
            try
            {
                if (Directory.Exists(tmpPath))
                {
                    Directory.Delete(tmpPath, true);
                }
                Directory.CreateDirectory(tmpPath);
                Directory.CreateDirectory(tmpPath + @"\Icon");
            }
            catch { }
            try
            {
                if (!Directory.Exists(commonPath))
                {
                    Directory.CreateDirectory(commonPath);
                    Directory.CreateDirectory(tmpPath);
                    Directory.CreateDirectory(tmpPath + @"\Icon");
                }
            }
            catch { }
        }


        internal void RemoveDevice(PackageComponentsComponentItemsDeviceDescription device)
        {
            if (Package.Components.Component.Items.DeviceDescription != null)
            {
                List<PackageComponentsComponentItemsDeviceDescription> deviceList = Package.Components.Component.Items.DeviceDescription.ToList();
                deviceList.Remove(device);
                Package.Components.Component.Items.DeviceDescription = deviceList.ToArray();
            }
            File.Delete(tmpPath + @"\" + device.Path);
        }

        internal void AddDevice()
        {
            string newDevice = null;
            FileInfo fileInfo = null;
            System.Windows.Forms.OpenFileDialog loadDialog = new System.Windows.Forms.OpenFileDialog();
            if (loadDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                newDevice = loadDialog.FileName;
                fileInfo = new FileInfo(newDevice);
            }
            if (fileInfo != null)
            {
                string deviceFolder = tmpPath + @"\Device\";
                Directory.CreateDirectory(deviceFolder);
                File.Copy(fileInfo.FullName, deviceFolder + fileInfo.Name, true);
                PackageComponentsComponentItemsDeviceDescription itemDevDesc = new PackageComponentsComponentItemsDeviceDescription();
                itemDevDesc.Path = @"Device\" + fileInfo.Name;
                DeviceDescription devDesc = SerializationExtensions.Deserialize<DeviceDescription>(XElement.Load(deviceFolder + fileInfo.Name));
                itemDevDesc.Version = devDesc.Device.DeviceIdentification.Version;
                itemDevDesc.DeviceId = devDesc.Device.DeviceIdentification.Id;
                itemDevDesc.DeviceType = devDesc.Device.DeviceIdentification.Type.ToString();
                AddLocalFiles(devDesc, fileInfo.DirectoryName + @"\", deviceFolder);

                List<PackageComponentsComponentItemsDeviceDescription> deviceList = new List<PackageComponentsComponentItemsDeviceDescription>();
                if (Package.Components.Component.Items.DeviceDescription != null)
                {
                    deviceList = Package.Components.Component.Items.DeviceDescription.ToList();
                }
                deviceList.Add(itemDevDesc);
                Package.Components.Component.Items.DeviceDescription = deviceList.ToArray();
            }
        }

        private void AddLocalFiles(DeviceDescription devDesc, string sourceFolder, string destinationFolder)
        {
            if (devDesc.Files != null && devDesc.Files.Language != null && devDesc.Files.Language.Count() > 0)
            {
                try
                {
                    foreach (var f in devDesc.Files.Language[0].File)
                    {
                        if (File.Exists(sourceFolder + f.Item))
                        {
                            File.Copy(sourceFolder + f.Item, destinationFolder + f.Item, true);
                        }
                    }
                }
                catch { }
            }
        }

        internal void RemoveFile(PackageComponentsComponentItemsFile file)
        {
            if (Package.Components.Component.Items.File != null)
            {
                List<PackageComponentsComponentItemsFile> fileList = Package.Components.Component.Items.File.ToList();
                fileList.Remove(file);
                PackageComponentsComponentItemsFile matchingTemplateFile = fileList.Where(f => f.Path == file.Path.Replace(".project", ".template")).FirstOrDefault();
                if (matchingTemplateFile == null)
                {
                    matchingTemplateFile = fileList.Where(f => f.Path == file.Path.Replace(".template", ".project")).FirstOrDefault();
                }
                if (matchingTemplateFile != null)
                {
                    fileList.Remove(matchingTemplateFile);
                    FileInfo matchingTemplateFileInfo = new FileInfo(tmpPath + @"\" + matchingTemplateFile.Path);
                    if (File.Exists(matchingTemplateFileInfo.FullName))
                    {
                        File.Delete(matchingTemplateFileInfo.FullName);
                    }
                }
                Package.Components.Component.Items.File = fileList.ToArray();
            }
            FileInfo fileInfo = new FileInfo(tmpPath + @"\" + file.Path);
            if (file.TargetFolder == "%AP_COMMON%")
            {
                if (file.IsInterface)
                {
                    if (File.Exists(fileInfo.FullName))
                    {
                        File.Delete(fileInfo.FullName);
                    }
                }
                else
                {
                    if (File.Exists(fileInfo.FullName))
                    {
                        Directory.Delete(fileInfo.DirectoryName, true);
                    }
                }
            }
            else
            {
                if (File.Exists(fileInfo.FullName))
                {
                    File.Delete(fileInfo.FullName);
                }
                if (fileInfo.Directory.EnumerateFiles().Count() == 0 && fileInfo.Directory.EnumerateDirectories().Count() == 0)
                {
                    fileInfo.Directory.Delete();
                }
            }
        }

        internal void EditFolder(PackageComponentsComponentItemsFolder folder)
        {
            NewFolder newFolder = new NewFolder();
            FolderProxy folderProxy = new FolderProxy(folder);
            newFolder.DataContext = folderProxy;
            bool? r = newFolder.ShowDialog();
            if (r.HasValue)
            {
                if (r.Value)
                {
                    folder.TargetFolder = folderProxy.TargetFolder;
                }
            }
        }

        internal void RemoveFolder(PackageComponentsComponentItemsFolder folder)
        {
            if (Package.Components.Component.Items.Folder != null)
            {
                List<PackageComponentsComponentItemsFolder> folderList = Package.Components.Component.Items.Folder.ToList();
                folderList.Remove(folder);
                Package.Components.Component.Items.Folder = folderList.ToArray();
            }
            Directory.Delete(tmpPath + @"\" + folder.Path, true);
        }

        internal void EditIcon()
        {
            string fileName = null;
            FileInfo fileInfo = null;
            System.Windows.Forms.OpenFileDialog loadDialog = new System.Windows.Forms.OpenFileDialog();
            loadDialog.Filter = "Icon .ico Files (.ico)|*.ico|All Files (*.*)|*.*";
            if (loadDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                fileName = loadDialog.FileName;
                fileInfo = new FileInfo(fileName);
                string path = @"Icon\";
                Directory.CreateDirectory(tmpPath + @"\" + path);
                path += fileInfo.Name;
                File.Copy(fileInfo.FullName, tmpPath + @"\" + path, true);
                Package.General.Icon = path;
            }
        }

        public enum FileType
        {
            Interface,
            Template,
            FolderHierarchy
        }
        internal PackageComponentsComponentItemsFolder AddFolder()
        {
            PackageComponentsComponentItemsFolder folder = null;
            NewFolder newFolder = new NewFolder();
            FolderProxy folderProxy = new FolderProxy();
            newFolder.DataContext = folderProxy;
            bool? r = newFolder.ShowDialog();
            if (r.HasValue)
            {
                if (r.Value)
                {
                    DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(folderProxy.SourceFolder);

                    CopyDirectory(sourceDirectoryInfo.FullName, tmpPath + @"\" + sourceDirectoryInfo.Name);
                    DirectoryInfo destinationDirectoryInfo = new DirectoryInfo(tmpPath + @"\" + sourceDirectoryInfo.Name);
                    List<PackageComponentsComponentItemsFolder> folderList = new List<PackageComponentsComponentItemsFolder>();
                    if (Package.Components.Component.Items.Folder != null)
                    {
                        folderList = Package.Components.Component.Items.Folder.ToList();
                    }
                    folder = new PackageComponentsComponentItemsFolder();
                    folder.TargetFolder = folderProxy.TargetFolder;
                    folder.Path = destinationDirectoryInfo.Name;
                    folder.FolderHierarchy.Add(new UserDirectory(tmpPath + folder.Path));
                    folderList.Insert(0, folder);
                    Package.Components.Component.Items.Folder = folderList.ToArray();
                }
            }
            return folder;
        }
        internal void AddFile(FileType fileType)
        {

            if (fileType == FileType.FolderHierarchy)
            {
                string folderName = null;
                System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    folderName = folderDialog.SelectedPath;
                    DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(folderName);

                    CopyDirectory(sourceDirectoryInfo.FullName, tmpPath + @"\" + sourceDirectoryInfo.Name);
                    DirectoryInfo destinationDirectoryInfo = new DirectoryInfo(tmpPath + @"\" + sourceDirectoryInfo.Name);

                    List<PackageComponentsComponentItemsFile> fileList = new List<PackageComponentsComponentItemsFile>();
                    if (Package.Components.Component.Items.File != null)
                    {
                        fileList = Package.Components.Component.Items.File.ToList();
                    }
                    PackageComponentsComponentItemsFile file;// = new PackageComponentsComponentItemsFile();
                    foreach (var f in destinationDirectoryInfo.EnumerateFiles("*", SearchOption.AllDirectories))
                    {
                        file = new PackageComponentsComponentItemsFile();
                        file.FileType = fileType;
                        file.TargetFolder = "%AP_ROOT%";
                        file.Path = destinationDirectoryInfo.Name + f.FullName.Replace(tmpPath + @"\" + destinationDirectoryInfo.Name, string.Empty);
                        file.IsInterface = false;
                        fileList.Insert(0, file);
                    }
                    Package.Components.Component.Items.File = fileList.ToArray();
                }
            }
            else
            {
                string fileName = null;
                FileInfo fileInfo = null;
                System.Windows.Forms.OpenFileDialog loadDialog = new System.Windows.Forms.OpenFileDialog();
                if (fileType == FileType.Interface)
                {
                    loadDialog.Filter = "Interface dll Files (.dll)|*.dll|All Files (*.*)|*.*";
                }
                if (fileType == FileType.Template)
                {
                    loadDialog.Filter = "Codesys Project Files (.project)|*.project|All Files (*.*)|*.*";
                }

                if (loadDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    fileName = loadDialog.FileName;
                    fileInfo = new FileInfo(fileName);
                }
                if (fileType == FileType.Interface)
                {
                    //string path = @"Install\" + Path.GetFileNameWithoutExtension(fileInfo.Name);
                    string path = @"Install\";// @"Install\" + Path.GetFileNameWithoutExtension(fileInfo.Name);
                    if (!Directory.Exists(tmpPath + @"\" + path))
                    {
                        Directory.CreateDirectory(tmpPath + @"\" + path);
                    }
                    //path += @"\" + fileInfo.Name;
                    path += fileInfo.Name;
                    File.Copy(fileInfo.FullName, tmpPath + @"\" + path, true);
                    PackageComponentsComponentItemsFile file = new PackageComponentsComponentItemsFile();
                    file.FileType = fileType;
                    file.TargetFolder = @"%AP_COMMON%";
                    file.Path = path;
                    file.IsInterface = true;
                    file.Version = GetFileVersion(tmpPath + @"\" + path);
                    List<PackageComponentsComponentItemsFile> fileList = new List<PackageComponentsComponentItemsFile>();
                    if (Package.Components.Component.Items.File != null)
                    {
                        fileList = Package.Components.Component.Items.File.ToList();
                    }
                    fileList.Insert(0, file);
                    Package.Components.Component.Items.File = fileList.ToArray();
                }
                if (fileType == FileType.Template)
                {
                    string path = "";
                    if (!Directory.Exists(tmpPath + @"\" + path))
                    {
                        Directory.CreateDirectory(tmpPath + @"\" + path);
                    }


                    ProjectTemplate projectTemplate = new ProjectTemplate();
                    projectTemplate.DefaultFileName = new ProjectTemplateDefaultFileName() { DefaultString = fileInfo.Name };
                    projectTemplate.TemplatePath = fileInfo.Name;
                    projectTemplate.Description = new ProjectTemplateDescription() { DefaultString = "A project containing one drive, one application and a Basic Speed control POU called from PLC_PRG" };
                    projectTemplate.Folder = new ProjectTemplateFolder() { DefaultString = @"Parker" };
                    projectTemplate.Name = new ProjectTemplateName() { DefaultString = Path.GetFileNameWithoutExtension(fileInfo.Name) + " Template Project" };

                    NewProjectTemplate newProjectTemplate = new NewProjectTemplate();
                    newProjectTemplate.DataContext = projectTemplate;
                    newProjectTemplate.ShowDialog();

                    List<PackageComponentsComponentItemsFile> fileList = new List<PackageComponentsComponentItemsFile>();
                    if (Package.Components.Component.Items.File != null)
                    {
                        fileList = Package.Components.Component.Items.File.ToList();
                    }
                    //path += @"\" + fileInfo.Name;
                    path = fileInfo.Name;
                    File.Copy(fileInfo.FullName, tmpPath + @"\" + path, true);
                    PackageComponentsComponentItemsFile file = new PackageComponentsComponentItemsFile();
                    file.FileType = fileType;
                    file.TargetFolder = @"%AP_ROOT%\Templates\" + projectTemplate.Folder.DefaultString;
                    file.Path = path;
                    fileList.Add(file);

                    path = path.Replace(".project", ".template");
                    projectTemplate.Serialize().Save(tmpPath + @"\" + path);
                    PackageComponentsComponentItemsFile file2 = new PackageComponentsComponentItemsFile();
                    file2.TargetFolder = @"%AP_ROOT%\Templates\" + projectTemplate.Folder.DefaultString;
                    file2.Path = path;
                    fileList.Add(file2);

                    Package.Components.Component.Items.File = fileList.ToArray();
                }
            }
        }

        internal void RemoveLibrary(PackageComponentsComponentItemsLibrary file)
        {
            if (Package.Components.Component.Items.Library != null)
            {
                List<PackageComponentsComponentItemsLibrary> fileList = Package.Components.Component.Items.Library.ToList();
                fileList.Remove(file);
                Package.Components.Component.Items.Library = fileList.ToArray();
            }
            FileInfo fileInfo = new FileInfo(tmpPath + @"\" + file.Path);
            if (File.Exists(fileInfo.FullName))
            {
                Directory.Delete(fileInfo.DirectoryName, true);
            }
        }

        internal void AddLibrary()
        {
            string libraryFileName = null;
            FileInfo fileInfo = null;

            System.Windows.Forms.OpenFileDialog loadDialog = new System.Windows.Forms.OpenFileDialog();
            loadDialog.Filter = "Library .library Files (.library)|*.library|All Files (*.*)|*.*";
            if (loadDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                libraryFileName = loadDialog.FileName;
                fileInfo = new FileInfo(libraryFileName);
            }
            if (fileInfo != null)
            {
                LibraryProxy libraryProxy = new LibraryProxy();
                libraryProxy.Name = Path.GetFileNameWithoutExtension(fileInfo.Name);
                libraryProxy.Version = "Version";
                NewLibrary newLibrary = new NewLibrary();
                newLibrary.DataContext = libraryProxy;
                bool? r = newLibrary.ShowDialog();
                if (r.HasValue)
                {
                    if (r.Value)
                    {
                        string path = @"Libraries\" + libraryProxy.Name + @"\" + libraryProxy.Version;
                        if (!Directory.Exists(tmpPath + @"\" + path))
                        {
                            Directory.CreateDirectory(tmpPath + @"\" + path);
                        }
                        path += @"\" + fileInfo.Name;
                        File.Copy(fileInfo.FullName, tmpPath + @"\" + path, true);
                        PackageComponentsComponentItemsLibrary library = new PackageComponentsComponentItemsLibrary();
                        library.Path = path;
                        List<PackageComponentsComponentItemsLibrary> libraryList = new List<PackageComponentsComponentItemsLibrary>();
                        if (Package.Components.Component.Items.Library != null)
                        {
                            libraryList = Package.Components.Component.Items.Library.ToList();
                        }
                        libraryList.Add(library);
                        Package.Components.Component.Items.Library = libraryList.ToArray();
                    }
                }
            }
        }

        internal void AddPlugin()
        {
            string pluginFileName = null;
            FileInfo fileInfo = null;

            System.Windows.Forms.OpenFileDialog loadDialog = new System.Windows.Forms.OpenFileDialog();
            loadDialog.Filter = "Plugin dll Files (.dll)|*.dll|All Files (*.*)|*.*";
            if (loadDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pluginFileName = loadDialog.FileName;
                fileInfo = new FileInfo(pluginFileName);
            }
            if (fileInfo != null)
            {
                PlugInProxy plugInProxy = new PlugInProxy();
                string[] tmp = Path.GetFileNameWithoutExtension(fileInfo.Name).Split('.');
                if (tmp.Count() > 2)
                {
                    plugInProxy.Name = tmp[tmp.Count() - 2];
                }
                else
                {
                    plugInProxy.Name = Path.GetFileNameWithoutExtension(fileInfo.Name);
                }
                plugInProxy.Version = GetFileVersion(fileInfo.FullName);
                Guid pluginGuid = Guid.Empty;
                List<AssemblyName> assys = GetPluginGuid(fileInfo.FullName, out pluginGuid);
                AddAssembliesToRegister(fileInfo.Name, pluginGuid, assys);

                plugInProxy.PlugInGuid = pluginGuid;
                NewPlugin newPlugin = new NewPlugin();
                newPlugin.DataContext = plugInProxy;
                bool? r = newPlugin.ShowDialog();
                if (r.HasValue)
                {
                    if (r.Value)
                    {
                        string path = @"Install\" + plugInProxy.Name;
                        if (Directory.Exists(tmpPath + @"\" + path))
                        {
                            Directory.Delete(tmpPath + @"\" + path, true);
                        }
                        if (!Directory.Exists(tmpPath + @"\" + path))
                        {
                            Directory.CreateDirectory(tmpPath + @"\" + path);
                        }
                        CopyDirectory(fileInfo.DirectoryName, tmpPath + @"\" + path);
                        path += @"\" + fileInfo.Name;
                        string pluginVersion = GetFileVersion(fileInfo.FullName);
                        GetPluginGuid(fileInfo.FullName, out pluginGuid);
                        if (pluginGuid != Guid.Empty)
                        {

                            PackageComponentsComponentItemsPlugIn plugin = new PackageComponentsComponentItemsPlugIn();
                            plugin.Path = path;
                            plugin.PlugInGuid = pluginGuid;
                            plugin.Version = pluginVersion;

                            List<PackageComponentsComponentItemsPlugIn> fileList = new List<PackageComponentsComponentItemsPlugIn>();
                            if (Package.Components.Component.Items.PlugIn != null)
                            {
                                fileList = Package.Components.Component.Items.PlugIn.ToList();
                            }
                            fileList.Add(plugin);
                            Package.Components.Component.Items.PlugIn = fileList.ToArray();

                            PackageComponentsComponentItemsProfileChange pluginProfileEntry = new PackageComponentsComponentItemsProfileChange();
                            pluginProfileEntry.PlugIn = "{" + pluginGuid.ToString() + "}";
                            pluginProfileEntry.Version = pluginVersion;
                            List<PackageComponentsComponentItemsProfileChange> profileList = new List<PackageComponentsComponentItemsProfileChange>();
                            if (Package.Components.Component.Items.ProfileChange != null)
                            {
                                profileList = Package.Components.Component.Items.ProfileChange.ToList();
                            }
                            profileList.Add(pluginProfileEntry);
                            Package.Components.Component.Items.ProfileChange = profileList.ToArray();
                        }
                        else
                        {
                            MessageBox.Show("Invalid Plugin");
                        }
                    }
                }
            }
        }

        private string GetFileVersion(string filePath)
        {
            if (File.Exists(filePath))
            {
                FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
                return myFileVersionInfo.FileVersion;
            }
            return string.Empty;
        }

        private List<AssemblyName> GetPluginGuid(string path, out Guid plugInGuid)
        {
            AppDomain tempAppDomain = null;
            List<AssemblyName> assys = new List<AssemblyName>();
            try
            {
                AppDomainSetup domainSetup = new AppDomainSetup();
                domainSetup = AppDomain.CurrentDomain.SetupInformation;
                domainSetup.ApplicationName = "TempDomain";
                tempAppDomain = AppDomain.CreateDomain("TempDomain",
                                        AppDomain.CurrentDomain.Evidence, domainSetup);
                LoadAssemblyAttributesProxy proxy = tempAppDomain.CreateInstanceAndUnwrap(
                                        Assembly.GetAssembly(typeof(LoadAssemblyAttributesProxy)).FullName, typeof(LoadAssemblyAttributesProxy).ToString())
                                         as LoadAssemblyAttributesProxy;
                plugInGuid = proxy.GetPluginGuidFromAssembly(path);
                assys = proxy.GetDependencies(path).ToList();
            }
            finally
            {
                if (tempAppDomain != null)
                {
                    AppDomain.Unload(tempAppDomain);
                    GC.Collect();
                }
            }
            return assys;
        }

        internal void RemovePlugin(PackageComponentsComponentItemsPlugIn file)
        {
            if (Package.Components.Component.Items.PlugIn != null)
            {
                List<PackageComponentsComponentItemsPlugIn> fileList = Package.Components.Component.Items.PlugIn.ToList();
                fileList.Remove(file);
                Package.Components.Component.Items.PlugIn = fileList.ToArray();
            }
            FileInfo fileInfo = new FileInfo(tmpPath + @"\" + file.Path);
            if (File.Exists(fileInfo.FullName))
            {
                Directory.Delete(fileInfo.DirectoryName, true);
            }
            if (Package.Components.Component.Items.ProfileChange != null)
            {
                List<PackageComponentsComponentItemsProfileChange> profileList = Package.Components.Component.Items.ProfileChange.ToList();
                PackageComponentsComponentItemsProfileChange profileItemToDelete = profileList.Where(p => p.PlugIn.ToLowerInvariant() == "{" + file.PlugInGuid.ToString().ToLowerInvariant() + "}").FirstOrDefault();
                if (profileItemToDelete != null)
                {
                    profileList.Remove(profileItemToDelete);
                    Package.Components.Component.Items.ProfileChange = profileList.ToArray();
                }
            }
            RemoveAssembliesFromRegisterUsingParentGuid(file.PlugInGuid);
        }

        private void CopyDirectory(string sourcePath, string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            foreach (string file in Directory.GetFiles(sourcePath))
            {
                string dest = Path.Combine(destPath, Path.GetFileName(file));
                File.Copy(file, dest, true);
            }

            foreach (string folder in Directory.GetDirectories(sourcePath))
            {
                string dest = Path.Combine(destPath, Path.GetFileName(folder));
                CopyDirectory(folder, dest);
            }
        }

        internal void AddHelpFile()
        {
            string helpFileFileName = null;
            FileInfo fileInfo = null;

            System.Windows.Forms.OpenFileDialog loadDialog = new System.Windows.Forms.OpenFileDialog();
            loadDialog.Filter = "Help Files .chm Files (.chm)|*.chm|All Files (*.*)|*.*";
            if (loadDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                helpFileFileName = loadDialog.FileName;
                fileInfo = new FileInfo(helpFileFileName);
            }
            if (fileInfo != null)
            {
                HelpFileProxy helpFileProxy = new HelpFileProxy();
                helpFileProxy.Culture = "en";
                helpFileProxy.FileName = fileInfo.Name;
                NewHelpFile newHelpFile = new NewHelpFile();
                newHelpFile.DataContext = helpFileProxy;
                bool? r = newHelpFile.ShowDialog();
                if (r.HasValue)
                {
                    if (r.Value)
                    {
                        string path = @"Help\" + helpFileProxy.Culture;
                        if (!Directory.Exists(tmpPath + @"\" + path))
                        {
                            Directory.CreateDirectory(tmpPath + @"\" + path);
                        }
                        path += @"\" + fileInfo.Name;
                        File.Copy(fileInfo.FullName, tmpPath + @"\" + path, true);
                        PackageComponentsComponentItemsOnlineHelpFile helpFile = new PackageComponentsComponentItemsOnlineHelpFile();
                        helpFile.Culture = helpFileProxy.Culture;
                        helpFile.Path = path;
                        List<PackageComponentsComponentItemsOnlineHelpFile> helpFileList = new List<PackageComponentsComponentItemsOnlineHelpFile>();
                        if (Package.Components.Component.Items.OnlineHelpFile != null)
                        {
                            helpFileList = Package.Components.Component.Items.OnlineHelpFile.ToList();
                        }
                        helpFileList.Add(helpFile);
                        Package.Components.Component.Items.OnlineHelpFile = helpFileList.ToArray();

                    }
                }
            }
        }

        internal void RemoveHelpFile(PackageComponentsComponentItemsOnlineHelpFile helpFile)
        {
            if (Package.Components.Component.Items.OnlineHelpFile != null)
            {
                List<PackageComponentsComponentItemsOnlineHelpFile> fileList = Package.Components.Component.Items.OnlineHelpFile.ToList();
                fileList.Remove(helpFile);
                Package.Components.Component.Items.OnlineHelpFile = fileList.ToArray();
            }
            FileInfo fileInfo = new FileInfo(tmpPath + @"\" + helpFile.Path);
            File.Delete(fileInfo.FullName);
            if (fileInfo.Directory.EnumerateFiles().Count() == 0 && fileInfo.Directory.EnumerateDirectories().Count() == 0)
            {
                Directory.Delete(fileInfo.DirectoryName, true);
            }
        }

        internal void RemoveAddMenuCommand(PackageComponentsComponentItemsAddMenuCommand addMenuCommand)
        {
            if (Package.Components.Component.Items.AddMenuCommand != null)
            {
                List<PackageComponentsComponentItemsAddMenuCommand> addMenuList = Package.Components.Component.Items.AddMenuCommand.ToList();
                addMenuList.Remove(addMenuCommand);
                Package.Components.Component.Items.AddMenuCommand = addMenuList.ToArray();
            }
        }

        internal void AddMenuCommand()
        {
            PackageComponentsComponentItemsAddMenuCommand addCmd = new PackageComponentsComponentItemsAddMenuCommand();
            addCmd.Command = "{" + Guid.Empty.ToString() + "}";
            addCmd.InsertionPath = "$FileMenu";
            PackageComponentsComponentItemsAddMenuCommandInsertionPosition addCmdPos = new PackageComponentsComponentItemsAddMenuCommandInsertionPosition();
            addCmdPos.Where = "After";
            addCmdPos.Command = "{" + Guid.Empty.ToString() + "}";
            addCmd.InsertionPosition = addCmdPos;

            NewAddCommand newAddCommand = new NewAddCommand();
            newAddCommand.DataContext = addCmd;
            bool? r = newAddCommand.ShowDialog();
            if (r.HasValue)
            {
                if (r.Value)
                {
                    List<PackageComponentsComponentItemsAddMenuCommand> addCommandList = new List<PackageComponentsComponentItemsAddMenuCommand>();
                    if (Package.Components.Component.Items.AddMenuCommand != null)
                    {
                        addCommandList = Package.Components.Component.Items.AddMenuCommand.ToList();
                    }
                    addCommandList.Add(addCmd);
                    Package.Components.Component.Items.AddMenuCommand = addCommandList.ToArray();
                }
            }

        }

        internal string DocumentRevisions()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("PACKAGE Version : " + this.Package.General.Version);
            sb.AppendLine();
            sb.AppendLine("Plugins");
            sb.AppendLine(String.Format("{0,-50}", "Plugin Name") + String.Format("{0,-40}", "Plugin Guid") + "Version");
            if (Package.Components.Component.Items.PlugIn != null)
            {
                foreach (var p in this.Package.Components.Component.Items.PlugIn)
                {
                    string pluginDllName = p.Path;
                    if (pluginDllName.Contains('\\'))
                    {
                        string[] tmp = p.Path.Split(new char[] { '\\' });
                        pluginDllName = tmp[tmp.Length - 1];
                    }
                    sb.AppendLine(String.Format("{0,-50}", pluginDllName) + String.Format("{0,-40}", p.PlugInGuid.ToString()) + p.Version);

                }
            }
            sb.AppendLine();

            sb.AppendLine("Interfaces");
            sb.AppendLine(String.Format("{0,-100}", "Name") + String.Format("{0,-40}", "Version"));
            if (Package.Components.Component.Items.File != null)
            {
                foreach (var p in this.Package.Components.Component.Items.File)
                {
                    if (p.IsInterface)
                    {
                        string interfaceName = p.Path;
                        if (interfaceName.Contains('\\'))
                        {
                            string[] tmp = p.Path.Split(new char[] { '\\' });
                            interfaceName = tmp[tmp.Length - 1];
                        }
                        sb.AppendLine(String.Format("{0,-60}", interfaceName) + String.Format("{0,-40}", p.Version));
                    }
                }

                sb.AppendLine();

                sb.AppendLine("Other Files");
                sb.AppendLine(String.Format("{0,-100}", "Name"));// + String.Format("{0,-40}", "Plugin Guid") + "Version");
                foreach (var p in this.Package.Components.Component.Items.File)
                {
                    if (!p.IsInterface)
                    {
                        string fileName = p.Path;
                        if (fileName.Contains('\\'))
                        {
                            string[] tmp = p.Path.Split(new char[] { '\\' });
                            fileName = tmp[tmp.Length - 1];
                        }
                        sb.AppendLine(String.Format("{0,-60}", fileName));
                    }
                }
                sb.AppendLine();
            }

            if(Package.Components.Component.Items.Library != null)
            {
                sb.AppendLine("Libraries");
                sb.AppendLine(String.Format("{0,-100}", "Library Name"));// + String.Format("{0,-40}", "Plugin Guid") + "Version");
                foreach (var p in this.Package.Components.Component.Items.Library)
                {
                    string libraryName = p.Path;
                    string libraryRevision = "";
                    if (libraryName.Contains('\\'))
                    {
                        string[] tmp = p.Path.Split(new char[] { '\\' });
                        libraryName = tmp[tmp.Length - 1];
                        libraryRevision = tmp[tmp.Length - 2];
                    }
                    sb.AppendLine(String.Format("{0,-60}", libraryName) + String.Format("{0,-40}", libraryRevision));
                }
                sb.AppendLine();
            }


            sb.AppendLine("Dependencies");
            sb.AppendLine(String.Format("{0,-40}", "Name"));// + String.Format("{0,-40}", "Plugin Guid") + "Version");
            foreach (var p in this.AssemblyNameVersionRegister.OrderBy(k => k.Key))
            {
                sb.Append(String.Format("{0,-40}", p.Key));
                List<string> assys = p.Value.Split(';').ToList();
                foreach (string s in assys)
                {
                    sb.AppendLine(String.Format("{0,-80}", s));
                    sb.Append(String.Format("{0,-40}", ""));
                }
                sb.AppendLine();
            }
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
