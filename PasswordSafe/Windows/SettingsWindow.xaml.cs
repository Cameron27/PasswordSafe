using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MahApps.Metro;
using MahApps.Metro.Controls;

namespace PasswordSafe
{
    /// <summary>
    ///     Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();

            if (ThemeManager.DetectAppStyle(Application.Current).Item1 == ThemeManager.GetAppTheme("BaseDark"))
                DarkModeToggle.IsChecked = true;
            AccentSelector.SelectedValue = ThemeManager.DetectAppStyle(Application.Current).Item2;
            FontSelector.SelectedValue = Application.Current.Resources["MainFont"];
            FontSizeSelector.Value = (double) Application.Current.Resources["MainFontSize"];
        }

        /// <summary>
        ///     Changes the programs AccentColor
        /// </summary>
        private void ChangeProgramsAccent(object sender, SelectionChangedEventArgs e)
        {
            Accent selectedAccent = AccentSelector.SelectedItem as Accent;
            if (selectedAccent != null)
            {
                Tuple<AppTheme, Accent> theme = ThemeManager.DetectAppStyle(Application.Current);
                ThemeManager.ChangeAppStyle(Application.Current, selectedAccent, theme.Item1);
            }
        }

        /// <summary>
        ///     Creates a new thread that toggles the program between light and dark mode
        /// </summary>
        private void ToggleDarkMode(object sender, EventArgs e)
        {
            //Creates a new thread to make the change smoother, still results in stuttering
            Thread changeAppThemeThread = new Thread(ChangeAppTheme);
            changeAppThemeThread.Start();
        }

        /// <summary>
        ///     Changes the programs font
        /// </summary>
        private void ChangeProgramsFont(object sender, SelectionChangedEventArgs e)
        {
            Application.Current.Resources["MainFont"] = FontSelector.SelectedItem as FontFamily;
        }

        /// <summary>
        ///     Applies the font size change when enter is pressed
        /// </summary>
        private void ApplyFontChangeOnEnterPress(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            if (FontSizeSelector.Value <= 32 && FontSizeSelector.Value > 0)
            {
                Application.Current.Resources["MainFontSize"] = FontSizeSelector.Value;
                Application.Current.Resources["LargerFontSize"] = FontSizeSelector.Value + 2;
            }
        }

        /// <summary>
        ///     Toggles the program between light and dark mode
        /// </summary>
        private void ChangeAppTheme()
        {
            Tuple<AppTheme, Accent> theme = ThemeManager.DetectAppStyle(this);
            //Runs the change back in the main threat
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                new Action(
                    () =>
                        ThemeManager.ChangeAppStyle(Application.Current, theme.Item2,
                            ThemeManager.GetAppTheme("Base" + (DarkModeToggle.IsChecked == true ? "Dark" : "Light")))));
        }
    }
}