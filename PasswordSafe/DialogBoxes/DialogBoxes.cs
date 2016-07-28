using MahApps.Metro.Controls;

namespace PasswordSafe.DialogBoxes
{
    public static class DialogBox
    {
        /// <summary>
        ///     Dialog box to inform user of an error
        /// </summary>
        /// <param name="errorMessage">Error message</param>
        /// <param name="owner">Owner of the window</param>
        public static void ErrorMessageDialogBox(string errorMessage, MetroWindow owner)
        {
            ErrorMessageDialogBox dialogBox = new ErrorMessageDialogBox(errorMessage)
            {
                Owner = owner
            };
            dialogBox.ShowDialog();
        }

        /// <summary>
        ///     Asks user a yes or no question and returns a boolean
        /// </summary>
        /// <param name="question">Question to display to the user</param>
        /// <param name="defaultYes">Is yes the default answer for when enter is pressed</param>
        /// <param name="owner">Owner of the window</param>
        public static bool QuestionDialogBox(string question, bool defaultYes, MetroWindow owner)
        {
            QuestionDialogBox dialogBox = new QuestionDialogBox(question, defaultYes) {Owner = owner};
            return (bool) dialogBox.ShowDialog();
        }

        /// <summary>
        ///     Dialog box to get a string input from user
        /// </summary>
        /// <param name="information">Information for user on what they need to input</param>
        /// <param name="confirm">Message for confirm button</param>
        /// <param name="cancel">Mesage for cancel button</param>
        /// <param name="owner">Owner of the window</param>
        public static string TextInputDialogBox(string information, string confirm, string cancel, MetroWindow owner)
        {
            TextInputDialogBox dialogBox = new TextInputDialogBox(information, confirm, cancel) {Owner = owner};

            if (dialogBox.ShowDialog() == true)
                return dialogBox.Answer;

            return null;
        }
    }
}