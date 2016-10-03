using System.Windows;
using MahApps.Metro.Controls;

namespace PasswordSafe.DialogBoxes
{
    /// <summary>
    ///     Interaction logic for MessageDialogBox.xaml
    /// </summary>
    public partial class MessageDialogBox : MetroWindow
    {
        /// <summary>
        ///     Dialog box to inform user of an error
        /// </summary>
        /// <param name="message">Message</param>
        public MessageDialogBox(string message)
        {
            InitializeComponent();
            Information.Text = message;
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