using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using AMS.Profile;
using MahApps.Metro;
using MahApps.Metro.Controls;
using PasswordSafe.DialogBoxes;
using static PasswordSafe.GlobalClasses.ModifySettings;

namespace PasswordSafe.Windows
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
            LoadSafeOptions();
            SetStyle();
            SetFont();
        }

        /// <summary>
        ///     Sets the app theme to be whatever is saved
        /// </summary>
        private void SetStyle()
        {
            ChangeProgramsAccent(ThemeManager.GetAccent(Profile.GetValue("Global", "Accent", "Blue")));
            ChangeProgramsTheme(Profile.GetValue("Global", "Theme", "BaseLight") == "BaseLight");
        }

        /// <summary>
        ///     Sets the app font and fonts size to whatever is saved
        /// </summary>
        private void SetFont()
        {
            ChangeProgramsFont(new FontFamily(Profile.GetValue("Global", "MainFont", "Arial")));
            ChangeProgramsFontSize(double.Parse(Profile.GetValue("Global", "MainFontSize", "12")));
        }

        /// <summary>
        ///     Loads all of the avaliable safe files
        /// </summary>
        private void LoadSafeOptions(string fileToAutoSelect = null)
        {
            string[] files = Directory.GetFiles(@"Resources", "*.json");
            //Takes the name of each json file e.g. C:/Users/John/Documents/Safe/test.json => test
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
            string name = DialogBox.TextInputDialogBox("Please enter the name for your new Safe:", "Create", "Cancel",
                this);

            if (string.IsNullOrEmpty(name)) return;

            if (SafeSelector.Items.Cast<string>().Any(x => x == name) &&
                !DialogBox.QuestionDialogBox(
                    "A file with that name already exists, are you sure you want to override it?", false, this))
                return;

            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                DialogBox.MessageDialogBox("A file's name cannot contain any of the following characters:\n\\/:*?\"<>|", this);
                return;
            }

            File.Create($"Resources\\{name}.json").Close();
            LoadSafeOptions(name);
        }

        /// <summary>
        ///     Logs into the password safe
        /// </summary>
        private void LoginOnClick(object sender, RoutedEventArgs e)
        {
            //Check if file still exists
            if (!File.Exists($"Resources\\{SafeSelector.SelectedValue}.json"))
            {
                LoadSafeOptions();
                DialogBox.MessageDialogBox("This safe does not exist", this);
                return;
            }

            MainWindow mainWindow = new MainWindow($"{SafeSelector.SelectedValue}.json");
            try
            {
                mainWindow.Show();
            }
            catch (InvalidOperationException)
            {
                //The window mush have already closed itself for some reason and an error has already been displayed to the user
            }
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