using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MahApps.Metro;
using MahApps.Metro.Controls;
using PasswordSafe.Properties;

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
            FontSelector.SelectedValue = Settings.Default.MainFont;
            FontSizeSelector.Value = Settings.Default.MainFontSize;
        }

        /// <summary>
        ///     Changes the programs AccentColor
        /// </summary>
        private void ChangeProgramsAccent(object sender, SelectionChangedEventArgs e)
        {
            Accent selectedAccent = (Accent) AccentSelector.SelectedItem;
            if (selectedAccent != null)
            {
                Tuple<AppTheme, Accent> currentStyle = ThemeManager.DetectAppStyle(Application.Current);
                //Used to keep the theme the same 
                ThemeManager.ChangeAppStyle(Application.Current, selectedAccent, currentStyle.Item1);

                //Saves the accent in settings
                Settings.Default.Accent = ((Accent) AccentSelector.SelectedItem).Name;
            }
        }

        /// <summary>
        ///     Creates a new thread that toggles the program between light and dark mode
        /// </summary>
        private void DarkModeToggled(object sender, EventArgs e)
        {
            //Creates a new thread to make the change smoother, still results in stuttering
            Thread changeAppThemeThread = new Thread(ChangeAppTheme);
            changeAppThemeThread.Start();
            //Saves the theme in the settings
            Settings.Default.Theme = DarkModeToggle.IsChecked == true ? "BaseDark" : "BaseLight";
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
                            ThemeManager.GetAppTheme(DarkModeToggle.IsChecked == true ? "BaseDark" : "BaseLight"))));
        }

        /// <summary>
        ///     Changes the programs font
        /// </summary>
        private void ChangeProgramsFont(object sender, SelectionChangedEventArgs e)
        {
            Settings.Default.MainFont = FontSelector.SelectedItem as FontFamily;
        }

        /// <summary>
        ///     Applies the font size change when the value of the selector is changed
        /// </summary>
        private void FontSizeSelector_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (FontSizeSelector.Value != null)
            {
                Settings.Default.MainFontSize = (double) FontSizeSelector.Value;
                Settings.Default.LargerFontSize = (double) (FontSizeSelector.Value + 2);
            }
        }
    }
}