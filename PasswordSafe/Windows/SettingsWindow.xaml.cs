using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using AMS.Profile;
using MahApps.Metro;
using MahApps.Metro.Controls;

namespace PasswordSafe.Windows
{
    /// <summary>
    ///     Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        private static readonly Xml Profile = new Xml("config.xml");
        private readonly Thread _idleDetectionThread;

        public SettingsWindow()
        {
            InitializeComponent();

            if (ThemeManager.DetectAppStyle(Application.Current).Item1 == ThemeManager.GetAppTheme("BaseDark"))
                DarkModeToggle.IsChecked = true;
            AccentSelector.SelectedValue = ThemeManager.DetectAppStyle(Application.Current).Item2;
            FontSelector.SelectedValue = Application.Current.Resources["MainFont"];
            FontSizeSelector.Value = (double?) Application.Current.Resources["MainFontSize"];
            LockTimeSelector.Value = MainWindow.TimeToLock;

            //Creates a thread that will check if the user is idle
            _idleDetectionThread = new Thread(CloseWindowOnLock);
            _idleDetectionThread.Start();
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

        /// <summary>
        /// Applies the auto lock time change when the value of the selector is changed
        /// </summary>
        private void LockTimeSelector_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            double newTime = (double) LockTimeSelector.Value;
            MainWindow.TimeToLock = newTime;
            Profile.SetValue("Global", "LockTime", newTime);
        }

        /// <summary>
        ///     Closes the window if the safe locks itself
        /// </summary>
        private void CloseWindowOnLock()
        {
            while (true)
            {
                Thread.Sleep(1000);

                IdleTimeInfo idleTime = IdleTimeDetector.GetIdleTimeInfo();

                if (idleTime.IdleTime.TotalMinutes >= MainWindow.TimeToLock && MainWindow.TimeToLock != 0)
                {
                    Dispatcher.Invoke(Close);
                    return;
                }
            }
        }

        /// <summary>
        ///     Runs final commands before closing
        /// </summary>
        private void MetroWindowClosing(object sender, CancelEventArgs e)
        {
            //Stops idle detection tread
            _idleDetectionThread.Abort();
        }
    }
}