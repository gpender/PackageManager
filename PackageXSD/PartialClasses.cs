using PackageManager.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using static PackageManager.ViewModels.MainViewModel;

public partial class PackageComponentsComponentItemsDeviceDescription : ViewModelBase
{
    private string version = string.Empty;
    private string id = string.Empty;
    private string type = string.Empty;

    [XmlIgnoreAttribute]
    public string DeviceId
    {
        get { return id; }
        set { id = value; }
    }

    [XmlIgnoreAttribute]
    public string DeviceType
    {
        get { return type; }
        set { type = value; }
    }

    [XmlIgnoreAttribute]
    public string Version
    {
        get { return version; }
        set {
            version = value;
            OnPropertyChanged("Version");
        }
    }
}

public class UserDirectory
{
    public ObservableCollection<UserFile> Files { get; set; } = new ObservableCollection<UserFile>();
    public ObservableCollection<UserDirectory> SubFolders { get; set; } = new ObservableCollection<UserDirectory>();
    public IEnumerable Items { get { return SubFolders?.Cast<Object>().Concat(Files); } }
    public string DirectoryPath { get; set; }
    public string Name { get { return System.IO.Path.GetFileName(DirectoryPath); } }

    public UserDirectory(string folderName)
    {
        if (folderName.Length <= 256)
        {
            DirectoryPath = folderName;
            foreach (var d in System.IO.Directory.EnumerateDirectories(folderName))
            {
                SubFolders.Add(new UserDirectory(d));
            }
            foreach (var d in System.IO.Directory.EnumerateFiles(folderName))
            {
                Files.Add(new UserFile(d));
            }
        }
    }
}
public class UserFile
{
    public string FilePath { get; set; }
    public string Name { get { return System.IO.Path.GetFileName(FilePath); } }
    public UserFile(string filePath)
    {
        FilePath = filePath;
    }
}

public partial class PackageComponentsComponentItemsFolder
{
    ObservableCollection<UserDirectory> folderHierarchy = new ObservableCollection<UserDirectory>();
    [XmlIgnoreAttribute]
    public ObservableCollection<UserDirectory> FolderHierarchy { get => folderHierarchy; }

}

public partial class PackageComponentsComponentItemsFile
{
    private string version = string.Empty;
    private bool isInterface = false;
    PackageManager.Model.CodesysPackageManager.FileType fileType;

    [XmlIgnoreAttribute]
    public bool IsInterface
    {
        get { return isInterface; }
        set { isInterface = value; }
    }

    [XmlIgnoreAttribute]
    public PackageManager.Model.CodesysPackageManager.FileType FileType
    {
        get { return fileType; }
        set { fileType = value; }
    }
    
    [XmlIgnoreAttribute]
    public string Version
    {
        get { return version; }
        set { version = value; }
    }
}

public partial class PackageComponentsComponentItemsPlugIn
{
    private Guid pluginGuid = Guid.Empty;
    private string version = string.Empty;

    [XmlIgnoreAttribute]
    public Guid PlugInGuid
    {
        get { return pluginGuid; }
        set { pluginGuid = value; }
    }

    [XmlIgnoreAttribute]
    public string Version
    {
        get { return version; }
        set { version = value; }
    }
}


