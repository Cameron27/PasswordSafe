using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace PasswordSafe
{
    /// <summary>
    ///     Interaction logic for ErrorMessage.xaml
    /// </summary>
    public partial class ErrorMessage : MetroWindow
    {
        public ErrorMessage(string information)
        {
            InitializeComponent();
            Information.Content = information;
        }

        /// <summary>
        ///     Closes the message when enter is pressed
        /// </summary>
        private void ConfirmOnEnterPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Close();
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