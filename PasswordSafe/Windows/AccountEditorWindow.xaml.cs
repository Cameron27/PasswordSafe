﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using PasswordSafe.DialogBoxes;
using PasswordSafe.GlobalClasses.CustomControls;
using PasswordSafe.GlobalClasses.Data;

namespace PasswordSafe.Windows
{
    /// <summary>
    ///     Interaction logic for AccountEditorWindow.xaml
    /// </summary>
    public partial class AccountEditorWindow : MetroWindow
    {
        private readonly List<string> _folders = new List<string>();

        public AccountEditorWindow(bool addAccount, Account accountToModify)
        {
            InitializeComponent();

            CreateFolderList(MainWindow.SafeData.Folders);

            AccountBeingEdited = accountToModify;
            if (addAccount)
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
            FolderField.SelectedIndex = _folders.IndexOf(account.Path);
            NotesField.Text = account.Notes;
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
            //TODO I broke it all or it never worked
            //Creates initial blank option
            if (depth == 0)
            {
                FolderField.Items.Add(new FolderComboBoxItem
                {
                    Content = "None",
                    Style = (Style) FindResource("FolderOptionsInContextMenu")
                });
                _folders.Add("");
            }

            IEnumerable<Folder> foldersEnumerable = folders as Folder[] ?? folders.ToArray();
            foreach (Folder folder in foldersEnumerable)
            {
                FolderField.Items.Add(new FolderComboBoxItem
                {
                    Content = $"{folder.Name}",
                    Indentation = depth * 20,
                    EndPath = foldersEnumerable.Last() == folder ? Visibility.Hidden : Visibility.Visible,
                    Style = (Style) FindResource("FolderOptionsInContextMenu")
                });
                _folders.Add($"{currentPath}/{folder.Name}");

                if (folder.Children.Count != 0)
                    CreateFolderList(folder.Children, $"{currentPath}/{folder.Name}", depth + 1);
            }
        }

        /// <summary>
        ///     Changes the value in FolderField so that it isn't the test selected but the folder path
        /// </summary>
        private void ReformatFolderOnFolderSelected(object sender, SelectionChangedEventArgs e)
        {
            Thread thread = new Thread(ReformatFolderThread);
            thread.Start();
        }

        /// <summary>
        ///     Changes the text in the FolderField after a 5ms delay
        /// </summary>
        private void ReformatFolderThread()
        {
            //Slight delay is needed because the selection changed event runs before the test value is updated
            Thread.Sleep(5);
            Dispatcher.Invoke(() => FolderField.Text = _folders[FolderField.SelectedIndex]);
        }

        /// <summary>
        ///     Checks if a path is an actual folder
        /// </summary>
        /// <param name="path">The folder path to verify</param>
        /// <returns>True if the folder exists</returns>
        private bool VerifyFolder(string path)
        {
            return _folders.Contains(path) || string.IsNullOrWhiteSpace(path);
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
        private void Confirm()
        {
            //Checks the passwords match
            if (PasswordField.Password != ConfirmPasswordField.Password)
            {
                DialogBox.ErrorMessageDialogBox("Your passwords do not match", this);
                return;
            }
            if (!VerifyFolder(FolderField.Text))
            {
                DialogBox.ErrorMessageDialogBox("That is not a valid folder", this);
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
            AccountBeingEdited.Notes = NotesField.Text;

            DialogResult = true;
            Close();
        }

        /// <summary>
        ///     Closes the window without updating the account's data
        /// </summary>
        private void CancelOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}