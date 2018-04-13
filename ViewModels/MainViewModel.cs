using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using PackageManager.Model;
using PackageManager.ViewModels.Command;
using PackageManager.Views;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using IconImage = System.Drawing.Icon;

namespace PackageManager.ViewModels
{
    class MainViewModel : ViewModelBase
    {
       
        RelayCommand readPackageCommand;
        RelayCommand createPackageCommand;
        RelayCommand createPackageGuidCommand;
        RelayCommand editIconCommand;
        RelayCommand documentRevisionsCommand;
        RelayCommand addDeviceCommand;
        RelayCommand<string> addFileCommand;
        RelayCommand addFolderCommand;
        RelayCommand addLibraryCommand;
        RelayCommand addPluginCommand;
        RelayCommand addHelpFileCommand;
        RelayCommand addAddMenuCommand;
        RelayCommand<PackageComponentsComponentDependenciesMinimumPlugInVersion> editPluginDependencyCommand;

        RelayCommand<PackageComponentsComponentItemsDeviceDescription> deleteDeviceCommand;
        RelayCommand<PackageComponentsComponentItemsFile> deleteFileCommand;
        RelayCommand<PackageComponentsComponentItemsFolder> deleteFolderCommand;
        RelayCommand<PackageComponentsComponentItemsFolder> editFolderCommand;
        RelayCommand<PackageComponentsComponentItemsLibrary> deleteLibraryCommand;
        RelayCommand<PackageComponentsComponentItemsPlugIn> deletePluginCommand;
        RelayCommand<PackageComponentsComponentItemsOnlineHelpFile> deleteHelpFileCommand;
        RelayCommand<PackageComponentsComponentItemsAddMenuCommand> deleteAddMenuCommand;


        CodesysPackageManager packageManager;

        public ObservableCollection<DependencyConflict> DependencyConflicts
        {
            get { return packageManager.DependencyConflicts; }
        }
            
        public PackageGeneral PackageGeneral
        {
            get { return packageManager.Package == null ? null : packageManager.Package.General; }
        }

        public PackageComponents PackageComponents
        {
            get { return packageManager.Package == null ? null : packageManager.Package.Components; }
        }

        public PackageComponentsComponentItemsDeviceDescription[] PackageComponentDevices
        {
            get { return packageManager.Package == null ? null : packageManager.Package.Components.Component.Items.DeviceDescription; }
        }

        public PackageComponentsComponentItemsFile[] PackageComponentInterfacesAndTemplates
        {
            get 
            {          
                try
                {
                    return packageManager.Package == null ? null : packageManager.Package.Components.Component.Items.File.Where(g => ((g.Path.Contains("Install") || g.Path.StartsWith("+") || g.Path.Contains("Template") || g.TargetFolder.Contains("Template")))).OrderBy(ob => ob.Path).ToArray();
                }
                catch { }
                return null;   
            }
        }

        public PackageComponentsComponentItemsFile[] PackageComponentFiles
        {
            get 
            {
                try
                {
                    return packageManager.Package == null ? null : packageManager.Package.Components.Component.Items.File.Where(g => !((g.Path.Contains("Install") || g.Path.Contains("+") || g.Path.Contains("Template") || g.TargetFolder.Contains("Template")))).ToArray();
                }
                catch { }
                return null;
            }
        }
        public PackageComponentsComponentItemsFolder[] PackageComponentFolders
        {
            get
            {
                try
                {
                    return packageManager.Package == null ? null : packageManager.Package.Components.Component.Items.Folder.ToArray();
                }
                catch { }
                return null;
            }
        }
        public PackageComponentsComponentDependenciesMinimumPlugInVersion[] PackageComponentDependencies
        {
            get 
            {
                //if (packageManager.Package == null)
                //{
                //    packageManager.Package = new Package();
                //}
                //if (packageManager.Package.Components == null)
                //{
                //    packageManager.Package.Components = new PackageComponents();
                //} 
                //if (packageManager.Package.Components.Component == null)
                //{
                //    packageManager.Package.Components.Component = new PackageComponentsComponent();
                //}
                //if (packageManager.Package.Components.Component.Dependencies == null)
                //{
                //    PackageComponentsComponentDependenciesMinimumPlugInVersion tmp = new PackageComponentsComponentDependenciesMinimumPlugInVersion();
                //    PackageComponentsComponentDependenciesMinimumPlugInVersion[] tmp2 = new PackageComponentsComponentDependenciesMinimumPlugInVersion[1];
                //    tmp2[0] = tmp;
                //    packageManager.Package.Components.Component.Dependencies = tmp2;
                //}
                return packageManager.Package == null ? null : packageManager.Package.Components.Component.Dependencies; 
            }
        }

        public PackageComponentsComponentItemsLibrary[] PackageComponentLibraries
        {
            get { return packageManager.Package == null ? null : packageManager.Package.Components.Component.Items.Library; }
        }

        public PackageComponentsComponentItemsPlugIn[] PackageComponentPlugins
        {
            get { return packageManager.Package == null ? null : packageManager.Package.Components.Component.Items.PlugIn; }
        }

        public PackageComponentsComponentItemsOnlineHelpFile[] PackageComponentOnlineHelp
        {
            get { return packageManager.Package == null ? null : packageManager.Package.Components.Component.Items.OnlineHelpFile; }
        }

        public PackageComponentsComponentItemsProfileChange[] PackageComponentProfileChange
        {
            get { return packageManager.Package == null ? null : packageManager.Package.Components.Component.Items.ProfileChange; }
        }

        public PackageComponentsComponentItemsAddMenuCommand[] PackageComponentAddMenuCommand
        {
            get { return packageManager.Package == null ? null : packageManager.Package.Components.Component.Items.AddMenuCommand; }
        }
  
        public MainViewModel()
        {
            packageManager = new CodesysPackageManager();
        }

        #region ReadPackageCommand

        public ICommand ReadPackageCommand
        {
            get
            {
                if (readPackageCommand == null)
                {
                    readPackageCommand = new RelayCommand(() => this.ReadPackage(), () => this.CanReadPackage);
                }
                return readPackageCommand;
            }
        }

        private void ReadPackage()
        {
            packageManager.ReadPackage();
            OnPropertyChanged("PackageGeneral");
            OnPropertyChanged("IconPath");
            OnPropertyChanged("PackageComponents");
            OnPropertyChanged("PackageComponentDevices");
            OnPropertyChanged("PackageComponentFiles");
            OnPropertyChanged("PackageComponentFolders");
            OnPropertyChanged("PackageComponentInterfacesAndTemplates");
            OnPropertyChanged("PackageComponentLibraries");
            OnPropertyChanged("PackageComponentPlugins");
            OnPropertyChanged("PackageComponentOnlineHelp");
            OnPropertyChanged("PackageComponentProfileChange");
            OnPropertyChanged("PackageComponentAddMenuCommand");
            OnPropertyChanged("PackageComponentDependencies");
        }

        bool CanReadPackage
        {
            get { return true; }
        }

        #endregion

        #region CreatePackageGuidCommand

        public ICommand CreatePackageGuidCommand
        {
            get
            {
                if (createPackageGuidCommand == null)
                {
                    createPackageGuidCommand = new RelayCommand(() => this.CreatePackageGuid(), () => this.CanCreatePackageGuid);
                }
                return createPackageGuidCommand;
            }
        }

        private void CreatePackageGuid()
        {
            packageManager.CreatePackageGuid();
            OnPropertyChanged("PackageGeneral");
        }

        bool CanCreatePackageGuid
        {
            get { return packageManager.Package != null; }
        }

        #endregion


        #region CreatePackageCommand

        public ICommand CreatePackageCommand
        {
            get
            {
                if (createPackageCommand == null)
                {
                    createPackageCommand = new RelayCommand(() => this.CreatePackage(), () => this.CanCreatePackage);
                }
                return createPackageCommand;
            }
        }

        private void CreatePackage()
        {
            packageManager.CreatePackage();
        }

        bool CanCreatePackage
        {
            get { return packageManager.Package != null; }
        }

        #endregion

        #region DocumentRevisionsCommand

        public ICommand DocumentRevisionsCommand
        {
            get
            {
                if (documentRevisionsCommand == null)
                {
                    documentRevisionsCommand = new RelayCommand(() => this.DocumentRevisions(), () => this.CanDocumentRevisions);
                }
                return documentRevisionsCommand;
            }
        }

        private void DocumentRevisions()
        {
            MessageCW cw = new MessageCW("Package Revision Summary", packageManager.DocumentRevisions());
            cw.Show();
        }

        bool CanDocumentRevisions
        {
            get { return packageManager.Package !=null; }
        }

        #endregion
        
        
        #region EditPluginDependencyCommand

        public ICommand EditPluginDependencyCommand
        {
            get
            {
                if (editPluginDependencyCommand == null)
                {
                    editPluginDependencyCommand = new RelayCommand<PackageComponentsComponentDependenciesMinimumPlugInVersion>((parameter) => this.EditPluginDependency(parameter));
                }
                return editPluginDependencyCommand;
            }
        }

        public void EditPluginDependency(PackageComponentsComponentDependenciesMinimumPlugInVersion dependency)
        {
            try
            {
                EditPluginDependency cw = new EditPluginDependency(); 
                cw.DataContext = dependency;
                if (cw.ShowDialog().Value)
                {
                    PackageComponentDependencies[0] = dependency;
                    OnPropertyChanged("PackageComponentDependencies");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        #endregion


        #region DeleteDeviceCommand

        public ICommand DeleteDeviceCommand
        {
            get
            {
                if (deleteDeviceCommand == null)
                {
                    deleteDeviceCommand = new RelayCommand<PackageComponentsComponentItemsDeviceDescription>((parameter) => this.DeleteDevice(parameter));
                }
                return deleteDeviceCommand;
            }
        }

        public void DeleteDevice(PackageComponentsComponentItemsDeviceDescription device)
        {
            try
            {
                packageManager.RemoveDevice(device);
                OnPropertyChanged("PackageComponentDevices");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        bool CanDeleteDevice
        {
            get { return true;}// packageManager.Package != null; }
        }

        #endregion

        #region AddDeviceCommand

        public ICommand AddDeviceCommand
        {
            get
            {
                if (addDeviceCommand == null)
                {
                    addDeviceCommand = new RelayCommand(() => this.AddDevice(), () => this.CanAddDevice);
                }
                return addDeviceCommand;
            }
        }

        public void AddDevice()
        {
            try
            {
                packageManager.AddDevice();
                OnPropertyChanged("PackageComponentDevices");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        bool CanAddDevice
        {
            get { return true; }// packageManager.Package != null; }
        }

        #endregion

        #region DeleteFileCommand

        public ICommand DeleteFileCommand
        {
            get
            {
                if (deleteFileCommand == null)
                {
                    deleteFileCommand = new RelayCommand<PackageComponentsComponentItemsFile>((parameter) => this.DeleteFile(parameter));
                }
                return deleteFileCommand;
            }
        }

        public void DeleteFile(PackageComponentsComponentItemsFile file)
        {
            try
            {
                packageManager.RemoveFile(file);
                OnPropertyChanged("PackageComponentFiles");
                OnPropertyChanged("PackageComponentInterfacesAndTemplates");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        bool CanDeleteFile
        {
            get { return true;}// packageManager.Package != null; }
        }

        #endregion

        #region AddFileCommand

        public ICommand AddFileCommand
        {
            get
            {
                if (addFileCommand == null)
                {
                    addFileCommand = new RelayCommand<string>((parameter) => this.AddFile(parameter));
                }
                return addFileCommand;
            }
        }

        public void AddFile(string fileTypeString)
        {
            try
            {
                PackageManager.Model.CodesysPackageManager.FileType fileType = (PackageManager.Model.CodesysPackageManager.FileType)Enum.Parse(typeof(PackageManager.Model.CodesysPackageManager.FileType), fileTypeString);
                packageManager.AddFile(fileType);
                OnPropertyChanged("PackageComponentFiles");
                OnPropertyChanged("PackageComponentFolders");
                OnPropertyChanged("PackageComponentInterfacesAndTemplates");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        bool CanAddFile
        {
            get { return true; }// packageManager.Package != null; }
        }

        #endregion

        #region DeleteFolderCommand

        public ICommand DeleteFolderCommand
        {
            get
            {
                if (deleteFolderCommand == null)
                {
                    deleteFolderCommand = new RelayCommand<PackageComponentsComponentItemsFolder>((parameter) => this.DeleteFolder(parameter));
                }
                return deleteFolderCommand;
            }
        }

        public void DeleteFolder(PackageComponentsComponentItemsFolder folder)
        {
            try
            {
                packageManager.RemoveFolder(folder);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            OnPropertyChanged("PackageComponentFolders");
        }

        bool CanDeleteFolder
        {
            get { return true; }// packageManager.Package != null; }
        }

        #endregion

        #region AddFolderCommand

        public ICommand AddFolderCommand
        {
            get
            {
                if (addFolderCommand == null)
                {
                    addFolderCommand = new RelayCommand(() => this.AddFolder(), () => this.CanAddFolder);
                }
                return addFolderCommand;
            }
        }

        public void AddFolder()
        {
            try
            {
                packageManager.AddFolder();
                OnPropertyChanged("PackageComponentFolders");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        bool CanAddFolder
        {
            get { return true; }// packageManager.Package != null; }
        }

        #endregion

        #region EditFolderCommand

        public ICommand EditFolderCommand
        {
            get
            {
                if (editFolderCommand == null)
                {
                    editFolderCommand = new RelayCommand<PackageComponentsComponentItemsFolder>((parameter) => this.EditFolder(parameter));
                }
                return editFolderCommand;
            }
        }

        public void EditFolder(PackageComponentsComponentItemsFolder folder)
        {
            packageManager.EditFolder(folder);
            OnPropertyChanged("PackageComponentFolders");
        }

        #endregion

        #region EditIconCommand

        public ICommand EditIconCommand
        {
            get
            {
                if (editIconCommand == null)
                {
                    editIconCommand = new RelayCommand(()=> this.EditIcon(), () => CanEditIcon());
                }
                return editIconCommand;
            }
        }
        public ImageSource IconPath
        {
            get
            {
                if (File.Exists(packageManager.IconPath))
                {
                    IconImage ico = IconImage.ExtractAssociatedIcon(packageManager.IconPath);
                    MemoryStream strm = new MemoryStream();
                    ico.Save(strm);
                    IconBitmapDecoder ibd = new IconBitmapDecoder(strm, BitmapCreateOptions.None, BitmapCacheOption.Default);
                    return ibd.Frames[0];
                }
                return null;
            }
        }
        public void EditIcon()
        {
            packageManager.EditIcon();
            OnPropertyChanged("PackageGeneral");
            OnPropertyChanged("IconPath");
        }
        public bool CanEditIcon()
        {
            return true;
        }




#endregion


#region DeleteLibraryCommand

public ICommand DeleteLibraryCommand
        {
            get
            {
                if (deleteLibraryCommand == null)
                {
                    deleteLibraryCommand = new RelayCommand<PackageComponentsComponentItemsLibrary>((parameter) => this.DeleteLibrary(parameter));
                }
                return deleteLibraryCommand;
            }
        }

        public void DeleteLibrary(PackageComponentsComponentItemsLibrary Library)
        {
            try
            {
                packageManager.RemoveLibrary(Library);
                OnPropertyChanged("PackageComponentLibraries");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        bool CanDeleteLibrary
        {
            get { return true; }// packageManager.Package != null; }
        }

        #endregion

        #region AddLibraryCommand

        public ICommand AddLibraryCommand
        {
            get
            {
                if (addLibraryCommand == null)
                {
                    addLibraryCommand = new RelayCommand(() => this.AddLibrary(), () => this.CanAddLibrary);
                }
                return addLibraryCommand;
            }
        }

        public void AddLibrary()
        {
            try
            {
                packageManager.AddLibrary();
                OnPropertyChanged("PackageComponentLibraries");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        bool CanAddLibrary
        {
            get { return true; }// packageManager.Package != null; }
        }

        #endregion

        #region DeletePluginCommand

        public ICommand DeletePluginCommand
        {
            get
            {
                if (deletePluginCommand == null)
                {
                    deletePluginCommand = new RelayCommand<PackageComponentsComponentItemsPlugIn>((parameter) => this.DeletePlugin(parameter));
                }
                return deletePluginCommand;
            }
        }

        public void DeletePlugin(PackageComponentsComponentItemsPlugIn plugin)
        {
            try
            {
                packageManager.RemovePlugin(plugin);
                OnPropertyChanged("PackageComponentPlugins");
                OnPropertyChanged("PackageComponentProfileChange");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        bool CanDeletePlugin
        {
            get { return true; }// packageManager.Package != null; }
        }

        #endregion

        #region AddPluginCommand

        public ICommand AddPluginCommand
        {
            get
            {
                if (addPluginCommand == null)
                {
                    addPluginCommand = new RelayCommand(() => this.AddPlugin(), () => this.CanAddPlugin);
                }
                return addPluginCommand;
            }
        }

        public void AddPlugin()
        {
            try
            {
                packageManager.AddPlugin();
                OnPropertyChanged("PackageComponentPlugins");
                OnPropertyChanged("PackageComponentProfileChange");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        bool CanAddPlugin
        {
            get { return true; }// packageManager.Package != null; }
        }

        #endregion

        #region DeleteHelpFileCommand

        public ICommand DeleteHelpFileCommand
        {
            get
            {
                if (deleteHelpFileCommand == null)
                {
                    deleteHelpFileCommand = new RelayCommand<PackageComponentsComponentItemsOnlineHelpFile>((parameter) => this.DeleteHelpFile(parameter));
                }
                return deleteHelpFileCommand;
            }
        }

        public void DeleteHelpFile(PackageComponentsComponentItemsOnlineHelpFile HelpFile)
        {
            try
            {
                packageManager.RemoveHelpFile(HelpFile);
                OnPropertyChanged("PackageComponentOnlineHelp");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        bool CanDeleteHelpFile
        {
            get { return true; }// packageManager.Package != null; }
        }

        #endregion

        #region AddHelpFileCommand

        public ICommand AddHelpFileCommand
        {
            get
            {
                if (addHelpFileCommand == null)
                {
                    addHelpFileCommand = new RelayCommand(() => this.AddHelpFile(), () => this.CanAddHelpFile);
                }
                return addHelpFileCommand;
            }
        }

        public void AddHelpFile()
        {
            try
            {
                packageManager.AddHelpFile();
                OnPropertyChanged("PackageComponentOnlineHelp");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        bool CanAddHelpFile
        {
            get { return true; }// packageManager.Package != null; }
        }

        #endregion

        #region DeleteMenuCommandCommand

        public ICommand DeleteAddMenuCommand
        {
            get
            {
                if (deleteAddMenuCommand == null)
                {
                    deleteAddMenuCommand = new RelayCommand<PackageComponentsComponentItemsAddMenuCommand>((parameter) => this.DeleteAddMenu(parameter));
                }
                return deleteAddMenuCommand;
            }
        }

        public void DeleteAddMenu(PackageComponentsComponentItemsAddMenuCommand addMenuCommand)
        {
            try
            {
                packageManager.RemoveAddMenuCommand(addMenuCommand);
                OnPropertyChanged("PackageComponentAddMenuCommand");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        bool CanDeleteMenuCommand
        {
            get { return true; }// packageManager.Package != null; }
        }

        #endregion

        #region AddAddMenuCommand

        public ICommand AddAddMenuCommand
        {
            get
            {
                if (addAddMenuCommand == null)
                {
                    addAddMenuCommand = new RelayCommand(() => this.AddAddMenu(), () => this.CanAddAddMenu);
                }
                return addAddMenuCommand;
            }
        }

        public void AddAddMenu()
        {
            try
            {
                packageManager.AddMenuCommand();
                OnPropertyChanged("PackageComponentAddMenuCommand");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        bool CanAddAddMenu
        {
            get { return true; }// packageManager.Package != null; }
        }

        #endregion

    }
}
