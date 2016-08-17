using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using PasswordSafe.CustomControls;
using PasswordSafe.Data;
using PasswordSafe.DialogBoxes;

namespace PasswordSafe.Windows
{
    /// <summary>
    ///     Interaction logic for AccountEditorWindow.xaml
    /// </summary>
    public partial class AccountEditorWindow : MetroWindow
    {
        private readonly List<string> _folders = new List<string>();
        private readonly Thread _idleDetectionThread;

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

            //Creates a thread that will check if the user is idle
            _idleDetectionThread = new Thread(CloseWindowOnLock);
            _idleDetectionThread.Start();
        }

        public Account AccountBeingEdited { get; }

        /// <summary>
        ///     Adds folders to the list of folders
        /// </summary>
        /// <param name="folders">List of folders to add</param>
        /// <param name="currentPath">Current path of the folder</param>
        /// <param name="depth">Current depth of folders</param>
        private void CreateFolderList(IEnumerable<Folder> folders, string currentPath = "", int depth = 0)
        {
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

        private void ReformatFolderOnFolderSelected(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            //Makes it so that this only run when somethig is selected from the dropdown
            if (!FolderField.IsDropDownOpen)
            {
                FolderField.SelectedItem = null;
                return;
            }

            Thread thread = new Thread(ReformatFolderThread);
            thread.Start();
        }

        /// <summary>
        ///     Changes the text in the FolderField after a 5ms delay
        /// </summary>
        private void ReformatFolderThread() //TODO Make this not horrible
        {
            Thread.Sleep(5);
            Application.Current.Dispatcher.Invoke(() => FolderField.Text = _folders[FolderField.SelectedIndex]);
        }

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
            FolderField.Text = account.Path;
            NotesField.Text = account.Notes;
        }

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
        ///     Checks if a path is an actual folder
        /// </summary>
        /// <param name="path">The folder path to verify</param>
        /// <returns>True if the folder exists</returns>
        private bool VerifyFolder(string path)
        {
            return _folders.Contains(path) || string.IsNullOrWhiteSpace(path);
        }

        /// <summary>
        ///     Closes the window without updating the account's data
        /// </summary>
        private void CancelOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     Closes the window if the safe locks itself
        /// </summary>
        private void CloseWindowOnLock()
        {
            while (true)
            {
                Thread.Sleep(1000);

                IdleTimeInfo idleTime = IdleTimeDetector.GetIdleTimeInfo();

                if (idleTime.IdleTime.TotalMinutes >= MainWindow.TimeToLock && MainWindow.TimeToLock != 0)
                {
                    Dispatcher.Invoke(Close);
                    return;
                }
            }
        }

        /// <summary>
        ///     Runs final commands before closing
        /// </summary>
        private void MetroWindowClosing(object sender, CancelEventArgs e)
        {
            //Stops idle detection tread
            _idleDetectionThread.Abort();
        }
    }
}