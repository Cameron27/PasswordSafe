using System.Windows;
using MahApps.Metro.Controls;
using PasswordSafe.Windows;

namespace PasswordSafe.DialogBoxes
{
    /// <summary>
    ///     Interaction logic for PasswordDialogBox.xaml
    /// </summary>
    public partial class PasswordDialogBox : MetroWindow
    {
        /// <summary>
        ///     Dialog box to get a password from the user
        /// </summary>
        /// <param name="information">Information to display above the password box</param>
        public PasswordDialogBox(string information)
        {
            InitializeComponent();

            Inforamtion.Content = information;
            DeterminePeakVisibility();
            PasswordInput.Focus();
        }

        public string Answer => PasswordInput.Password;

        /// <summary>
        ///     Determines is the peak button should be visible or not
        /// </summary>
        private void DeterminePeakVisibility()
        {
            if (MainWindow.Profile.GetValue("Advanced", "DisablePasswordPeaking", "false") == "true")
                PeakToggleButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        ///     Closes the window with a dialog result of true
        /// </summary>
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        ///     Toggles the password boxes to hide and show the text
        /// </summary>
        private void TogglePeakOnClick(object sender, RoutedEventArgs e)
        {
            bool? isChecked = PeakToggleButton.IsChecked;
            if ((isChecked != null) && (bool) isChecked)
            {
                PeakBox.Visibility = Visibility.Visible;
                PeakBox.Text = PasswordInput.Password;
            }
            else
                PeakBox.Visibility = Visibility.Collapsed;
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