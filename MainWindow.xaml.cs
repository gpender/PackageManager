using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Ionic.Zip;
using PackageManager.Serialization;
using PackageManager.ViewModels;

namespace PackageManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        StringCollection plugins = new StringCollection();
        StringCollection libraries = new StringCollection();
        StringCollection interfaces = new StringCollection();
        StringCollection devices = new StringCollection();

        public MainWindow()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
            }
            else
            {
                MainViewModel mainVM = new MainViewModel();//.Singleton;
                base.DataContext = mainVM;
            }
        }

        private void ReadFileList()
        {
            plugins = PackageManager.Properties.Settings.Default.PluginPaths;
            libraries = PackageManager.Properties.Settings.Default.LibraryPaths;
            interfaces = PackageManager.Properties.Settings.Default.InterfacePath;
            devices = PackageManager.Properties.Settings.Default.DevicePaths;
        }

        private void pluginsButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog loadDialog = new System.Windows.Forms.OpenFileDialog();
            if (loadDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //find the plugins GUID
                //find the plugins version
                //find the referenced GAC version
                //Add plugin to list
            }
        }

        private void librariesButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog loadDialog = new System.Windows.Forms.OpenFileDialog();
            if (loadDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //Add Library to list
            }
        }

        private void devicesButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog loadDialog = new System.Windows.Forms.OpenFileDialog();
            if (loadDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //Add Device to list
            }
        }

        private void interfaceButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog loadDialog = new System.Windows.Forms.OpenFileDialog();
            if (loadDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //Add Interface to list
            }
        }

        private void generateButton_Click(object sender, RoutedEventArgs e)
        {

        }

        #region Properties

        public StringCollection Plugins
        {
            get
            {
                return plugins;
            }
        }
        public StringCollection Libraries
        {
            get
            {
                return libraries;
            }
        }
        public StringCollection Interfaces
        {
            get
            {
                return interfaces;
            }
        }

        public StringCollection Devices
        {
            get
            {
                return devices;
            }
        }
        #endregion
    }
}
