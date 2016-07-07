using System.Windows;
using MahApps.Metro.Controls;

namespace PasswordSafe
{
    /// <summary>
    ///     Interaction logic for ErrorMessageDialogBox.xaml
    /// </summary>
    public partial class QuestionDialogBox : MetroWindow
    {
        /// <summary>
        /// Asks user a yes or no question and returns a boolean
        /// </summary>
        /// <param name="question">Question to display to the user</param>
        /// <param name="defaultYes">Is yes the default answer for when enter is pressed</param>
        public QuestionDialogBox(string question, bool defaultYes)
        {
            InitializeComponent();
            Information.Content = question;
            if (defaultYes)
            {
                YesButton.Focus();
            }
            else
            {
                NoButton.Focus();
            }
        }

        /// <summary>
        ///     Closes the message and returns true
        /// </summary>
        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        ///     Closes the message and returns false
        /// </summary>
        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}