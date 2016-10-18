using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using PasswordSafe.DialogBoxes;
using PasswordSafe.GlobalClasses;
using PasswordSafe.GlobalClasses.CustomControls;
using PasswordSafe.GlobalClasses.Data;

namespace PasswordSafe.Windows
{
    /// <summary>
    ///     Interaction logic for AccountEditorWindow.xaml
    /// </summary>
    public partial class AccountEditorWindow : MetroWindow
    {
        private readonly bool _addingNewAccount;
        private readonly Account _originalAccountClone;

        public AccountEditorWindow(bool addingNewAccount, Account accountToModify)
        {
            InitializeComponent();

            CreateFolderList(MainWindow.SafeData.Folders);

            AccountBeingEdited = accountToModify;
            _originalAccountClone = (Account) accountToModify.Clone();

            if (addingNewAccount)
            {
                Header.Content = "ADD ACCOUNT:";
                ComfirmButton.Content = "Create";
                AccountBeingEdited.Id = -1; //-1 is used so it can be realised the ID is not set
            }
            else
            {
                Header.Content = "EDIT ACCOUNT:";
                ComfirmButton.Content = "Apply";
                SetTextBoxValues(accountToModify);
            }

            DeterminePeakVisibility();

            _addingNewAccount = addingNewAccount;
        }

        public Account AccountBeingEdited { get; }

        #region Setup

        /// <summary>
        ///     Fills all the input boxes with the data from an account
        /// </summary>
        /// <param name="account">The account that the values are taken from</param>
        private void SetTextBoxValues(Account account)
        {
            AccountField.Text = account.AccountName;
            UsernameField.Text = account.Username;
            EmailField.Text = account.Email;
            PasswordField.Password = account.Password;
            ConfirmPasswordField.Password = account.Password;
            UrlField.Text = account.Url;
            FolderField.SelectedValue =
                FolderField.Items.OfType<FolderComboBoxItem>().First(x => (string) x.Content == account.Path);
            NotesField.Text = account.Notes;

            if (AccountBeingEdited.Backup)
            {
                RestoreButton.Visibility = Visibility.Visible;
                ComfirmButton.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        ///     Determines is the peak button should be visible or not
        /// </summary>
        private void DeterminePeakVisibility()
        {
            if (MainWindow.Profile.GetValue("Advanced", "DisablePasswordPeaking", "false") == "true")
                PeakToggleButton.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Folders

        /// <summary>
        ///     Adds folders to the list of folders
        /// </summary>
        /// <param name="folders">List of folders to add</param>
        /// <param name="currentPath">Current path of the folder</param>
        /// <param name="depth">Current depth of folders</param>
        private void CreateFolderList(IEnumerable<Folder> folders, string currentPath = "", int depth = 0)
        {
            //Creates initial blank option
            if (depth == 0)
                FolderField.Items.Add(new FolderComboBoxItem
                {
                    FolderName = "None",
                    Content = "",
                    Style = (Style) FindResource("FolderOptionsInContextMenu")
                });

            IEnumerable<Folder> foldersEnumerable = folders as Folder[] ?? folders.ToArray();
            foreach (Folder folder in foldersEnumerable)
            {
                FolderField.Items.Add(new FolderComboBoxItem
                {
                    FolderName = $"{folder.Name}",
                    Content = $"{currentPath}/{folder.Name}",
                    Indentation = depth * 20,
                    EndOfPath = foldersEnumerable.Last() == folder ? Visibility.Hidden : Visibility.Visible,
                    Style = (Style) FindResource("FolderOptionsInContextMenu")
                });

                if (folder.Children.Count != 0)
                    CreateFolderList(folder.Children, $"{currentPath}/{folder.Name}", depth + 1);
            }
        }

        /// <summary>
        ///     Checks if a path is an actual folder
        /// </summary>
        /// <param name="path">The folder path to verify</param>
        /// <returns>True if the folder exists</returns>
        private bool VerifyFolder(string path)
        {
            return FolderField.Items.OfType<FolderComboBoxItem>().Any(x => (string) x.Content == path) ||
                   string.IsNullOrWhiteSpace(path);
        }

        #endregion

        #region Closing

        /// <summary>
        ///     Sets values of accountBeingEdited to be equal to the values in the input boxes then closes the window
        /// </summary>
        private void ComfirmOnClick(object sender, RoutedEventArgs e)
        {
            Confirm();
        }

        /// <summary>
        ///     Sets values of accountBeingEdited to be equal to the values in the input boxes then closes the window when enter is
        ///     pressed
        /// </summary>
        private void ConfirmOnEnterPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Confirm();
        }

        /// <summary>
        ///     Sets values of accountBeingEdited to be equal to the values in the input boxes then closes the window
        /// </summary>
        private void Confirm(bool restoreBackup = false)
        {
            //Checks the passwords match
            if (PasswordField.Password != ConfirmPasswordField.Password)
            {
                DialogBox.MessageDialogBox("Your passwords do not match", this);
                return;
            }
            if (!VerifyFolder(FolderField.Text))
            {
                DialogBox.MessageDialogBox("That is not a valid folder", this);
                return;
            }

            //Sets ID as one higher than the highest ID currently being used for new accounts
            if (AccountBeingEdited.Id == -1)
                if (MainWindow.SafeData.Accounts.Count != 0)
                    AccountBeingEdited.Id = MainWindow.SafeData.Accounts.Last().Id + 1;
                else
                    AccountBeingEdited.Id = 0;
            AccountBeingEdited.AccountName = AccountField.Text;
            AccountBeingEdited.Username = UsernameField.Text;
            AccountBeingEdited.Email = EmailField.Text;
            AccountBeingEdited.Password = PasswordField.Password;
            AccountBeingEdited.Url = UrlField.Text;
            AccountBeingEdited.Path = FolderField.Text;
            if (_addingNewAccount)
                AccountBeingEdited.DateCreated = DateTime.Now.ToShortDateString();
            AccountBeingEdited.Notes = NotesField.Text;

            if (!_addingNewAccount)
            {
                if (!_originalAccountClone.Equals(AccountBeingEdited))
                {
                    DialogResult = true;
                    AccountBeingEdited.DateLastEdited = DateTime.Now.ToShortDateString();
                    //Create backup
                    if (MainWindow.Profile.GetValue("Advanced", "AutoBackup", "true") == "true")
                    {
                        _originalAccountClone.Id = MainWindow.SafeData.Accounts.Last().Id + 1;
                        _originalAccountClone.Backup = true;
                        ((MainWindow) Owner).AccountsObservableCollection.Add(_originalAccountClone);
                    }
                }

                if (restoreBackup)
                {
                    if ((DialogResult != null) && !DialogResult.Value)
                        DialogResult = true;
                    if (_originalAccountClone.Equals(AccountBeingEdited))
                        AccountBeingEdited.DateLastEdited = DateTime.Now.ToShortDateString();
                    AccountBeingEdited.Backup = false;
                }
            }

            if (_addingNewAccount)
            {
                DialogResult = true;
                AccountBeingEdited.DateLastEdited = DateTime.Now.ToShortDateString();
            }

            Close();
        }

        /// <summary>
        ///     Closes the window without updating the account's data
        /// </summary>
        private void CancelOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     Re-adds the entry to the safe
        /// </summary>
        private void RestoreBackup(object sender, RoutedEventArgs e)
        {
            Confirm(true);
        }

        #endregion

        #region Other

        /// <summary>
        ///     Toggles the password boxes to hide and show the text
        /// </summary>
        private void TogglePeakOnClick(object sender, RoutedEventArgs e)
        {
            bool? isChecked = PeakToggleButton.IsChecked;
            if ((isChecked != null) && (bool) isChecked)
            {
                PeakBox.Visibility = Visibility.Visible;
                ConfirmPeakBox.Visibility = Visibility.Visible;
                PeakBox.Text = PasswordField.Password;
                ConfirmPeakBox.Text = ConfirmPasswordField.Password;
            }
            else
            {
                PeakBox.Visibility = Visibility.Collapsed;
                ConfirmPeakBox.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        ///     Generates a random password
        /// </summary>
        private void GenerateRandomPasswordOnClick(object sender, RoutedEventArgs e)
        {
            string randomPassword = "";
            int passwordLength =
                (int) double.Parse(MainWindow.Profile.GetValue("Security", "RandomPasswordLength", "24"));

            Func<byte, bool> filter;
            if (MainWindow.Profile.GetValue("Security", "LimitPasswordCharacters", "true") == "true")
                filter = x => ((x >= 48) && (x <= 57)) || ((x >= 65) && (x <= 90)) || ((x >= 97) && (x <= 122));
            else
                filter = x => (x <= 126) && (x >= 32);

            while (randomPassword.Length < passwordLength)
                randomPassword +=
                    Encoding.ASCII.GetString(AESThenHMAC.NewKey().Where(filter).ToArray());
            randomPassword = randomPassword.Substring(0, passwordLength);

            PasswordField.Password = randomPassword;
            ConfirmPasswordField.Password = randomPassword;
            PeakBox.Text = randomPassword;
            ConfirmPeakBox.Text = randomPassword;
        }

        /// <summary>
        ///     Prevents pasting into the PasswordBox
        /// </summary>
        private void PreventPasting(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Paste)
                e.Handled = true;
        }

        #endregion
    }
}