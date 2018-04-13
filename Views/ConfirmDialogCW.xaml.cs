using System.Windows;
using System.Windows.Input;

namespace PackageManager.Views
{
    /// <summary>
    /// Interaction logic for ConfirmDialogCW.xaml
    /// </summary>
    public partial class ConfirmDialogCW : Window
    {
        public string Message
        {
            set { message.Text = value; }
        }
        public ConfirmDialogCW(string title, string message)
        {
            InitializeComponent();
            Title = title;
            Message = message;
            KeyDown += new System.Windows.Input.KeyEventHandler(NewEditNeedCW_KeyDown);
        }

        void NewEditNeedCW_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseWindow();
            }
            if (e.Key == Key.Enter)
            {
                DialogResult = true;
            }
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            DialogResult = false;
        }
    }
}
