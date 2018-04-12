using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;



public partial class PackageComponentsComponentItemsDeviceDescription
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
        set { version = value; }
    }
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


