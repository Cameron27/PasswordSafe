using System.Windows;
using MahApps.Metro.Controls;

namespace PasswordSafe.DialogBoxes
{
    /// <summary>
    ///     Interaction logic for ErrorMessageDialogBox.xaml
    /// </summary>
    public partial class ErrorMessageDialogBox : MetroWindow
    {
        /// <summary>
        ///     Dialog box to inform user of an error
        /// </summary>
        /// <param name="errorMessage">Error message</param>
        public ErrorMessageDialogBox(string errorMessage)
        {
            InitializeComponent();
            Information.Content = errorMessage;
        }

        /// <summary>
        ///     Closes the message
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}