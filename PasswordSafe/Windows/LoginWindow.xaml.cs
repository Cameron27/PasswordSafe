using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using AMS.Profile;
using MahApps.Metro;
using MahApps.Metro.Controls;

namespace PasswordSafe
{
    /// <summary>
    ///     Interaction logic for Login.xaml
    /// </summary>
    public partial class LoginWindow : MetroWindow
    {
        private static readonly Xml Profile = new Xml("config.xml");

        public LoginWindow()
        {
            InitializeComponent();

            PasswordInput.Focus();
            LoadSafeOptions(null);
            SetStyle();
            SetFont();
        }

        /// <summary>
        ///     Sets the app theme to be whatever is saved
        /// </summary>
        private void SetStyle()
        {
            Accent accent = ThemeManager.GetAccent(Profile.GetValue("Global", "Accent", "Blue"));
            AppTheme theme = ThemeManager.GetAppTheme(Profile.GetValue("Global", "Theme", "BaseLight"));
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme);
        }

        /// <summary>
        ///     Sets the app font and fonts size to whatever is saved
        /// </summary>
        private void SetFont()
        {
            Application.Current.Resources["MainFont"] = new FontFamily(Profile.GetValue("Global", "MainFont", "Arial"));
            Application.Current.Resources["MainFontSize"] =
                double.Parse(Profile.GetValue("Global", "MainFontSize", "12"));
            Application.Current.Resources["LargerFontSize"] =
                double.Parse(Profile.GetValue("Global", "MainFontSize", "12")) + 2;
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
        private void NewSafeOnClick(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<MetroWindow>().Any(x => x.Title == "AccountEditorWindow"))
                return; //Check if a account editor window is already open

            TextInputDialogBox fileNameDialogBox = new TextInputDialogBox("Please enter the name for your new Safe:",
                "Create",
                "Cancel") {Owner = this};

            if (fileNameDialogBox.ShowDialog() != true || fileNameDialogBox.Input.Text == "") return;
            string name = fileNameDialogBox.Input.Text;
            if (SafeSelector.Items.Cast<string>().Any(x => x == name))
            {
                ErrorMessageDialogBox error = new ErrorMessageDialogBox("A safe with that name already exists")
                {
                    Owner = this
                };
                error.ShowDialog();
                return;
            }
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                ErrorMessageDialogBox error = new ErrorMessageDialogBox("That is not a valid file name") {Owner = this};
                error.ShowDialog();
                return;
            }
            File.Create($"Resources\\{name}.json");
            LoadSafeOptions(name);
        }

        /// <summary>
        ///     Logs into the password safe
        /// </summary>
        private void LoginOnClick(object sender, RoutedEventArgs e)
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
        private void ExitOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}