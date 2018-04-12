using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PackageManager.Views
{
    /// <summary>
    /// Interaction logic for MessageCW.xaml
    /// </summary>
    public partial class MessageCW : Window
    {

        public MessageCW(string title, string message)
        {
            InitializeComponent();
            this.Title = title;
            this.message.Text = message;
            this.KeyDown += new System.Windows.Input.KeyEventHandler(NewEditNeedCW_KeyDown);
        }

        void NewEditNeedCW_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseWindow();
            }
            if (e.Key == Key.Enter)
            {
                this.DialogResult = true;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            this.Close();
        }
    }
}
