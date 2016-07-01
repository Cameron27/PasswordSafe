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

            LoadSafeOptions(null);
        }

        /// <summary>
        ///     Loads all of the avaliable safe files
        /// </summary>
        private void LoadSafeOptions(string fileToAutoSelect)
        {
            string[] files = Directory.GetFiles(@"Resources", "*.json");
            files = files.Select(x => x.Split('\\').Last().Split('.')[0]).ToArray();

            SafeSelector.ItemsSource = files;

            //Sets the selected file to either the new file or the first one
            if (fileToAutoSelect == null)
                SafeSelector.SelectedIndex = 0;
            else
                SafeSelector.SelectedValue = fileToAutoSelect;
        }

        /// <summary>
        ///     Shows a dialog box to get the name for the new safe and creates it
        /// </summary>
        private void NewSafe_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<MetroWindow>().Any(x => x.Title == "EntryEditorWindow"))
                return; //Check if a entry editor window is already open

            TextInputDialogBox fileNameDialogBox = new TextInputDialogBox("Please enter the name for your new Safe:",
                "Create", "Cancel")
            {
                Owner = this,
                Left = Left + ActualWidth / 2.0,
                Top = Top + ActualHeight / 2.0
            };

            if (fileNameDialogBox.ShowDialog() != true || fileNameDialogBox.Input.Text == "") return;
            string name = fileNameDialogBox.Input.Text;
            if (SafeSelector.Items.Cast<string>().Any(x => x == name))
            {
                ErrorMessage error = new ErrorMessage("A safe with that name already exists")
                {
                    Owner = this,
                    Left = Left + ActualWidth / 2.0,
                    Top = Top + ActualHeight / 2.0
                };
                error.ShowDialog();
                return;
            }
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                ErrorMessage error = new ErrorMessage("That is not a valid file name")
                {
                    Owner = this,
                    Left = Left + ActualWidth / 2.0,
                    Top = Top + ActualHeight / 2.0
                };
                error.ShowDialog();
                return;
            }
            File.Create($"Resources\\{name}.json");
            LoadSafeOptions(name);
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
        ///     Logs into the password safe
        /// </summary>
        private void LoginToSafe()
        {
            if (Application.Current.Windows.OfType<MetroWindow>().Any(x => x.Title == "MainWindow"))
                return; //Check if a settings window is already open

            MainWindow mainWindow = new MainWindow($"{SafeSelector.SelectedValue}.json");
            mainWindow.Show();
            Close();
        }

        /// <summary>
        ///     Closes the window
        /// </summary>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}