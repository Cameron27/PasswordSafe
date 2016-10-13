using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using PasswordSafe.DialogBoxes;
using PasswordSafe.GlobalClasses;
using PasswordSafe.GlobalClasses.Data;
using static PasswordSafe.GlobalClasses.ModifySettings;


namespace PasswordSafe.Windows
{
    /// <summary>
    ///     Interaction logic for Login.xaml
    /// </summary>
    public partial class LoginWindow : MetroWindow
    {
        public LoginWindow()
        {
            InitializeComponent();

            PasswordInput.Focus();
            LoadSafeOptions();
            SetStyle();
            SetFont();
            DeterminePeakVisibility();

            string lastSafeOpened = MainWindow.Profile.GetValue("General", "LastSafeOpened", null);
            if ((lastSafeOpened != null) && SafeSelector.Items.Contains(lastSafeOpened))
                SafeSelector.SelectedValue = lastSafeOpened;
        }

        /// <summary>
        ///     Sets the app theme to be whatever is saved
        /// </summary>
        private void SetStyle()
        {
            ChangeProgramsAccent(ThemeManager.GetAccent(MainWindow.Profile.GetValue("Appearance", "Accent", "Blue")));
            ChangeProgramsTheme(MainWindow.Profile.GetValue("Appearance", "Theme", "BaseLight") == "BaseLight");
        }

        /// <summary>
        ///     Sets the app font and fonts size to whatever is saved
        /// </summary>
        private void SetFont()
        {
            ChangeProgramsFont(new FontFamily(MainWindow.Profile.GetValue("Appearance", "Font", "Arial")));
            ChangeProgramsFontSize(double.Parse(MainWindow.Profile.GetValue("Appearance", "FontSize", "12")));
        }

        /// <summary>
        ///     Loads all of the avaliable safe files
        /// </summary>
        private void LoadSafeOptions(string fileToAutoSelect = null)
        {
            if (!Directory.Exists("Safes"))
                Directory.CreateDirectory("Safes");

            string[] files = Directory.GetFiles(@"Safes", "*.safe");
            //Takes the name of each json file e.g. C:/Users/John/Documents/Safe/test.safe => test
            files = files.Select(x => x.Split('\\').Last().Split('.')[0]).ToArray();

            SafeSelector.ItemsSource = files;

            //Sets the selected file to either the new file or the first one
            if (fileToAutoSelect == null)
                SafeSelector.SelectedIndex = 0;
            else
                SafeSelector.SelectedValue = fileToAutoSelect;
        }

        /// <summary>
        ///     Determines is the peak button should be visible or not
        /// </summary>
        private void DeterminePeakVisibility()
        {
            if (MainWindow.Profile.GetValue("Advanced", "DisablePasswordPeaking", "false") == "true")
                PeakToggleButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        ///     Shows a dialog box to get the name for the new safe and creates it
        /// </summary>
        private void NewSafeOnClick(object sender, RoutedEventArgs e)
        {
            //Gets new name
            string newName = DialogBox.TextInputDialogBox("Please enter the name for your new Safe:", "Create", "Cancel",
                this);

            if (string.IsNullOrEmpty(newName)) return;

            //Checks if name is valid
            if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                DialogBox.MessageDialogBox(
                    "A file's name cannot contain any of the following characters:\n\\/:*?\"<>|", this);
                return;
            }

            //Checks if that name already exists
            if (SafeSelector.Items.Cast<string>().Any(x => x == newName) &&
                !DialogBox.QuestionDialogBox(
                    "A file with that name already exists, are you sure you want to override it?", false, this))
                return;

            //Gets password
            string password;
            if (!GetAndConfirmNewPassword(out password, this)) return;

            //Creates new RootObject
            string versionNumber = string.Join(".",
                Assembly.GetExecutingAssembly().GetName().Version.ToString().Split('.').Take(2));
            RootObject baseObject = new RootObject
            {
                Folders = new List<Folder>(),
                Accounts = new List<Account>(),
                VersionNumber = versionNumber
            };

            //Turns it into json and encrypts it
            string jsonText = JsonConvert.SerializeObject(baseObject);
            string encryptedText = AESThenHMAC.SimpleEncryptWithPassword(jsonText, password);

            //Creates file
            File.Create($"Safes\\{newName}.safe").Close();
            File.WriteAllText($"Safes\\{newName}.safe", encryptedText);
            LoadSafeOptions(newName);
        }

        public static bool GetAndConfirmNewPassword(out string password, MetroWindow ownerForErrorMessages)
        {
            while (true)
            {
                password = "";
                while (password == "")
                {
                    password = DialogBox.PasswordDialogBox("Please enter the password for the new safe:",
                        ownerForErrorMessages);
                    if (password == null)
                        return false;

                    if (password == "")
                        DialogBox.MessageDialogBox("You must enter a password", ownerForErrorMessages);
                }

                string checkPassword = DialogBox.PasswordDialogBox("Please confirm your password:",
                    ownerForErrorMessages);
                if (checkPassword == password)
                    return true;

                if (checkPassword == null)
                    return false;

                DialogBox.MessageDialogBox("Those passwords do not match, please try again.", ownerForErrorMessages);
            }
        }

        /// <summary>
        ///     Logs into the password safe
        /// </summary>
        private void LoginOnClick(object sender, RoutedEventArgs e)
        {
            //Checks a password has been entered
            if (PasswordInput.Password.Length == 0)
            {
                DialogBox.MessageDialogBox("Please enter a password.", this);
                return;
            }

            string decryptedContent;
            bool loadingBackup = false;

            int result = TryLoadAndDecrypt($"Safes\\{SafeSelector.SelectedValue}.safe", out decryptedContent);
            if (result == 2)
            {
                result = TryLoadAndDecrypt($"Safes\\{SafeSelector.SelectedValue}.safe.bak", out decryptedContent);

                if (result != 0)
                {
                    DialogBox.MessageDialogBox(
                        "This safe appears to be corrupted and a backup could not be recovered.", this);
                    return;
                }
                loadingBackup = true;
            }
            else if (result == 1)
            {
                DialogBox.MessageDialogBox("There was an error trying to load that safe.", this);
                return;
            }

            //Checks password was correct
            if (decryptedContent == null)
            {
                DialogBox.MessageDialogBox("That password is incorrect, please try again.", this);
                return;
            }

            //Create SafeData
            RootObject safeData;
            try
            {
                safeData = JsonConvert.DeserializeObject<RootObject>(decryptedContent);
            }
            catch (JsonReaderException)
            {
                DialogBox.MessageDialogBox("That safe is not a valid file format.", this);
                return;
            }

            //Create MainWindow
            MainWindow mainWindow = new MainWindow($"{SafeSelector.SelectedValue}.safe", safeData,
                PasswordInput.Password);
            try
            {
                mainWindow.Show();
            }
            catch (InvalidOperationException)
            {
                return;
            }
            if (loadingBackup)
                DialogBox.MessageDialogBox("The safe appears to be corrupted and an older version has been loaded",
                    mainWindow);

            MainWindow.Profile.SetValue("General", "LastSafeOpened", SafeSelector.SelectedValue);

            Close();
        }

        /// <summary>
        ///     Loads and decrypts the safe
        /// </summary>
        /// <param name="safePath">The path the safe is located at</param>
        /// <param name="decryptedContent">Returns the content of the safe</param>
        /// <returns>
        ///     0 if the safe is loaded and decrypted, 1 if there is an error in loading the safe, 2 if there was an error in
        ///     decrypting the safe
        /// </returns>
        private int TryLoadAndDecrypt(string safePath, out string decryptedContent)
        {
            decryptedContent = null;

            //Check if file exists
            if (!File.Exists(safePath))
            {
                LoadSafeOptions();
                return 1;
            }

            //Loads content of file
            string contentOfFile;

            try
            {
                contentOfFile = File.ReadAllText(safePath);
            }
            catch (IOException)
            {
                return 1;
            }

            //Decrypts content
            try
            {
                decryptedContent = AESThenHMAC.SimpleDecryptWithPassword(contentOfFile, PasswordInput.Password);
            }
            catch (FormatException)
            {
                return 2;
            }

            return 0;
        }

        /// <summary>
        ///     Closes the window
        /// </summary>
        private void ExitOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     Toggles the password boxes to hide and show the text
        /// </summary>
        private void TogglePeakOnClick(object sender, RoutedEventArgs e)
        {
            bool? isChecked = PeakToggleButton.IsChecked;
            if ((isChecked != null) && (bool) isChecked)
            {
                PeakBox.Visibility = Visibility.Visible;
                PeakBox.Text = PasswordInput.Password;
            }
            else
                PeakBox.Visibility = Visibility.Collapsed;
        }
    }
}