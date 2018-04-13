using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PackageManager.CustomControls
{
    public enum TargetFolderOptions
    {
        ROOT,
        COMMON,
        Other_Folder
    }
    public class TargetFolderOptionsListView : ListView
    {
        public TargetFolderOptionsListView()
        {
            Type type = typeof(TargetFolderOptions);
            FieldInfo[] fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            List<TargetFolderOptions> list = new List<TargetFolderOptions>();
            foreach (FieldInfo info in fields)
            {
                TargetFolderOptions enum2 = (TargetFolderOptions)info.GetValue(null);
                list.Add(enum2);
            }
            this.ItemsSource = list;
        }
    }
}
