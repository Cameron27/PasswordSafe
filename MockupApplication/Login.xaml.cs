using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace MockupApplication
{
    /// <summary>
    ///     Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : MetroWindow
    {
        public Login()
        {
            InitializeComponent();
            string[] files = Directory.GetFiles(@"Resources", "*.json");
            files = files.Select(x => x.Split('\\').Last()).ToArray();

            SafeSelector.ItemsSource = files;
            SafeSelector.SelectedIndex = 0;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginToSafe();
        }

        private void PasswordIput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                LoginToSafe();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoginToSafe()
        {
            if (Application.Current.Windows.OfType<MetroWindow>().Any(x => x.Title == "MainWindow"))
                return; //Check if a settings window is already open

            MetroWindow settingsWindow = new MainWindow();
            settingsWindow.Left = Left + ActualWidth / 2.0;
            settingsWindow.Top = Top + ActualHeight / 2.0;
            settingsWindow.Show();
            Close();
        }
    }
}