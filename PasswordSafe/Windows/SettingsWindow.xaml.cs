using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using AMS.Profile;
using MahApps.Metro;
using MahApps.Metro.Controls;

namespace PasswordSafe
{
    /// <summary>
    ///     Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        private static readonly Xml Profile = new Xml("config.xml");

        public SettingsWindow()
        {
            InitializeComponent();

            if (ThemeManager.DetectAppStyle(Application.Current).Item1 == ThemeManager.GetAppTheme("BaseDark"))
                DarkModeToggle.IsChecked = true;
            AccentSelector.SelectedValue = ThemeManager.DetectAppStyle(Application.Current).Item2;
            FontSelector.SelectedValue = Application.Current.Resources["MainFont"];
            FontSizeSelector.Value = (double?) Application.Current.Resources["MainFontSize"];
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
                Profile.SetValue("Global", "Accent", ((Accent) AccentSelector.SelectedItem).Name);
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
            Profile.SetValue("Global", "Theme", DarkModeToggle.IsChecked == true ? "BaseDark" : "BaseLight");
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
            Application.Current.Resources["MainFont"] = (FontFamily) FontSelector.SelectedItem;
            Profile.SetValue("Global", "MainFont", (FontFamily) FontSelector.SelectedItem);
        }

        /// <summary>
        ///     Applies the font size change when the value of the selector is changed
        /// </summary>
        private void FontSizeSelector_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            if (FontSizeSelector.Value != null)
            {
                Application.Current.Resources["MainFontSize"] = FontSizeSelector.Value;
                Application.Current.Resources["LargerFontSize"] = FontSizeSelector.Value;
                Profile.SetValue("Global", "MainFontSize", FontSizeSelector.Value);
            }
        }
    }
}