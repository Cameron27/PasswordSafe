using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace PasswordSafe
{
    /// <summary>
    ///     Interaction logic for Login.xaml
    /// </summary>
    public partial class LoginWindow : MetroWindow
    {
        public LoginWindow()
        {
            InitializeComponent();
            string[] files = Directory.GetFiles(@"Resources", "*.json");
            files = files.Select(x => x.Split('\\').Last()).ToArray();

            SafeSelector.ItemsSource = files;
            SafeSelector.SelectedIndex = 0;
        }

        /// <summary>
        ///     Logs into the password safe
        /// </summary>
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginToSafe();
        }

        /// <summary>
        ///     Logs into the password safe when enter is presses
        /// </summary>
        private void LoginOnEnterPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                LoginToSafe();
        }

        /// <summary>
        ///     Closes the window
        /// </summary>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     Logs into the password safe
        /// </summary>
        private void LoginToSafe()
        {
            if (Application.Current.Windows.OfType<MetroWindow>().Any(x => x.Title == "MainWindow"))
                return; //Check if a settings window is already open

            MetroWindow mainWindow = new MainWindow();
            mainWindow.Left = Left + ActualWidth / 2.0;
            mainWindow.Top = Top + ActualHeight / 2.0;
            mainWindow.Show();
            Close();
        }
    }
}