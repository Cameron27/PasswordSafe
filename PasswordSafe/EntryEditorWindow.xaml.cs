using System.Linq;
using System.Windows;
using MahApps.Metro.Controls;
using PasswordSafe.Data;

namespace PasswordSafe
{
    /// <summary>
    ///     Interaction logic for EntryEditorWindow.xaml
    /// </summary>
    public partial class EntryEditorWindow : MetroWindow
    {
        public EntryEditorWindow(bool addEntry, Account accountToModify)
        {
            InitializeComponent();
            AccountBeingEdited = accountToModify;
            if (addEntry)
            {
                Header.Content = "ADD ENTRY:";
                ComfirmButton.Content = "Create";
                AccountBeingEdited.Id = -1; //-1 is used so it can be realised the ID is not set
            }
            else
            {
                Header.Content = "EDIT ENTRY:";
                ComfirmButton.Content = "Apply";
                SetTextBoxValues(accountToModify);
            }
        }

        public Account AccountBeingEdited { get; }

        /// <summary>
        ///     Fills all the input boxes with the data from an account
        /// </summary>
        /// <param name="account">The account that the values are taken from</param>
        private void SetTextBoxValues(Account account)
        {
            AccountEntry.Text = account.AccountName;
            UsernameEntry.Text = account.Username;
            EmailEntry.Text = account.Email;
            PasswordEntry.Text = account.Password;
            UrlEntry.Text = account.Url;
            NotesEntry.Text = account.Notes;
        }

        /// <summary>
        ///     Sets values of accountBeingEdited to be equal to the values in the input boxes then closes the window
        /// </summary>
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            //Sets ID as one higher than the highest ID currently being used for new accounts
            if (AccountBeingEdited.Id == -1)
                AccountBeingEdited.Id = MainWindow.SafeData.Accounts.Last().Id + 1;
            AccountBeingEdited.AccountName = AccountEntry.Text;
            AccountBeingEdited.Username = UsernameEntry.Text;
            AccountBeingEdited.Email = EmailEntry.Text;
            AccountBeingEdited.Password = PasswordEntry.Text;
            AccountBeingEdited.Url = UrlEntry.Text;
            AccountBeingEdited.Notes = NotesEntry.Text;

            DialogResult = true;
            Close();
        }

        /// <summary>
        ///     Closes the window without updating the account's data
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}