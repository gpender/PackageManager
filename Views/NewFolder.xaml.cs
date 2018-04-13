using PackageManager.Model;
using System.IO;
using System.Windows;

namespace PackageManager.Views
{
    /// <summary>
    /// Interaction logic for NewFolder.xaml
    /// </summary>
    public partial class NewFolder : Window
    {
        FolderProxy vm = null;

        public NewFolder()
        {
            InitializeComponent();
            this.DataContextChanged += NewFolder_DataContextChanged;
        }

        private void NewFolder_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(vm==null)
            {
                vm = this.DataContext as FolderProxy;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(vm.SourceFolder) && vm.NotEditMode)
            {
                e.Handled = false;
                this.error.Text = "Source Folder does not exist";
            }
            else
            {
                this.DialogResult = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                vm.SourceFolder = folderDialog.SelectedPath;
            }
        }
    }
}
