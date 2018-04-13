using PackageManager.CustomControls;
using PackageManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageManager.Model
{
    class FolderProxy : ViewModelBase
    {
        string sourceFolder;
        string targetFolder = @"c:\";
        TargetFolderOptions selectedTargetFolderOption = TargetFolderOptions.Other_Folder;
        public bool NotEditMode { get; set; }
        public string TargetFolder
        {
            get => targetFolder;
            set
            {
                targetFolder = value;
                OnPropertyChanged("TargetFolder");
            }
        }
        public string SourceFolder
        {
            get => sourceFolder;
            set
            {
                sourceFolder = value;
                OnPropertyChanged("SourceFolder");
            }
        }
        public TargetFolderOptions SelectedTargetFolderOption
        {
            get => selectedTargetFolderOption;
            set
            {
                selectedTargetFolderOption = value;
                switch(selectedTargetFolderOption)
                {
                    case TargetFolderOptions.COMMON:
                        TargetFolder = "%AP_COMMON%";
                        break;
                    case TargetFolderOptions.ROOT:
                        TargetFolder = "%AP_ROOT%";
                        break;
                    case TargetFolderOptions.Other_Folder:
                        TargetFolder = @"c:\";
                        break;
                }
                OnPropertyChanged("SelectedTargetFolderOption");
            }
        }

        public FolderProxy() { NotEditMode = true; }
        public FolderProxy(PackageComponentsComponentItemsFolder folder)
        {
            NotEditMode = false;
            SourceFolder = folder.Path;
            if (folder.TargetFolder == "%AP_COMMON%")
            {
                this.SelectedTargetFolderOption = TargetFolderOptions.COMMON;
            }
            else if (folder.TargetFolder == "%AP_ROOT%")
            {
                this.SelectedTargetFolderOption = TargetFolderOptions.ROOT;
            }
            else
            {
                TargetFolder = folder.TargetFolder;
            }
        }
    }
}

