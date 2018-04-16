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


public class UserItem
{
    public string Name { get { return System.IO.Path.GetFileName(Path); } }
    public PackageComponentsComponentItemsFolder ParentItemsFolder { get; protected set; }
    public string Path { get; set; }
    public UserItem(string path, PackageComponentsComponentItemsFolder parentItemsFolder)
    {
        Path = path;
        ParentItemsFolder = parentItemsFolder;
    }

}
public class UserDirectory : UserItem
{
    public ObservableCollection<UserFile> Files { get; set; } = new ObservableCollection<UserFile>();
    public ObservableCollection<UserDirectory> SubFolders { get; set; } = new ObservableCollection<UserDirectory>();
    public IEnumerable Items { get { return SubFolders?.Cast<Object>().Concat(Files); } }

    public UserDirectory(string path, PackageComponentsComponentItemsFolder parentItemsFolder) : base(path,parentItemsFolder)
    {
        if (Path.Length <= 256)
        {
            foreach (var dir in System.IO.Directory.EnumerateDirectories(Path))
            {
                SubFolders.Add(new UserDirectory(dir, parentItemsFolder));
            }
            foreach (var file in System.IO.Directory.EnumerateFiles(path))
            {
                Files.Add(new UserFile(file, parentItemsFolder));
            }
        }
    }
}
public class UserFile : UserItem
{
    public UserFile(string path, PackageComponentsComponentItemsFolder parentItemsFolder) : base(path, parentItemsFolder) { }
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


