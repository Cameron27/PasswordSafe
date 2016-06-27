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
        public EntryEditorWindow()
        {
            InitializeComponent();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SafeData.Accounts.Add(new Account
            {
                AccountName = AccountEntry.Text,
                Username = UsernameEntry.Text,
                Email = EmailEntry.Text,
                Password = PasswordEntry.Text,
                Url = UrlEntry.Text,
                Notes = NotesEntry.Text
            });
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}