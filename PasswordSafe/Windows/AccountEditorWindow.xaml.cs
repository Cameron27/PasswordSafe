using System.Linq;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using PasswordSafe.Data;

namespace PasswordSafe.Windows
{
    /// <summary>
    ///     Interaction logic for AccountEditorWindow.xaml
    /// </summary>
    public partial class AccountEditorWindow : MetroWindow
    {
        public AccountEditorWindow(bool addAccount, Account accountToModify)
        {
            InitializeComponent();
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
            UrlField.Text = account.Url;
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
    }
}