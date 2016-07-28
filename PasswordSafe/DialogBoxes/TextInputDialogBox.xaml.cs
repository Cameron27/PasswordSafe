using System.Windows;
using MahApps.Metro.Controls;

namespace PasswordSafe.DialogBoxes
{
    /// <summary>
    ///     Interaction logic for TextInputDialogBox.xaml
    /// </summary>
    public partial class TextInputDialogBox : MetroWindow
    {
        /// <summary>
        ///     Dialog box to get a string input from user
        /// </summary>
        /// <param name="information">Information for user on what they need to input</param>
        /// <param name="confirm">Message for confirm button</param>
        /// <param name="cancel">Mesage for cancel button</param>
        public TextInputDialogBox(string information, string confirm, string cancel)
        {
            InitializeComponent();

            Information.Content = information;
            ConfirmButton.Content = confirm;
            CancelButton.Content = cancel;

            Input.Focus();
        }

        public string Answer => Input.Text;

        /// <summary>
        ///     Closes the window with a dialog result of true
        /// </summary>
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        ///     Closes the window with a dialog result of false
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}