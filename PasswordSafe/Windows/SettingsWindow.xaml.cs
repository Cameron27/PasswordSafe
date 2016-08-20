﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AMS.Profile;
using MahApps.Metro;
using MahApps.Metro.Controls;
using static PasswordSafe.GlobalClasses.ModifySettings;

namespace PasswordSafe.Windows
{
    /// <summary>
    ///     Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        private const int NumberOfSettings = 5;
        private static readonly Xml Profile = new Xml("config.xml");
        private bool[] _modifiedSettings = new bool[NumberOfSettings];

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
            //_idleDetectionThread = new Thread(CloseWindowOnLock);
            //_idleDetectionThread.Start();
        }

        #region Settings Changed

        /// <summary>
        ///     Changes the programs AccentColor
        /// </summary>
        private void ChangeProgramsAccentLive(object sender, SelectionChangedEventArgs e)
        {
            _modifiedSettings[0] = true;

            ChangeProgramsAccent((Accent) AccentSelector.SelectedItem);
        }

        /// <summary>
        ///     Creates a new thread that toggles the program between light and dark mode
        /// </summary>
        private void ChangeProgramsThemeLive(object sender, EventArgs e)
        {
            _modifiedSettings[1] = true;

            ChangeProgramsTheme(DarkModeToggle.IsChecked != null && !(bool) DarkModeToggle.IsChecked);
        }

        /// <summary>
        ///     Changes the programs font
        /// </summary>
        private void ChangeProgramsFontLive(object sender, SelectionChangedEventArgs e)
        {
            _modifiedSettings[2] = true;

            ChangeProgramsFont((FontFamily) FontSelector.SelectedItem);
        }

        /// <summary>
        ///     Applies the font size change when the value of the selector is changed
        /// </summary>
        private void ChangeProgramsFontSizeLive(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            _modifiedSettings[3] = true;

            if (FontSizeSelector.Value != null)
            {
                ChangeProgramsFontSize((double) FontSizeSelector.Value);
            }
        }

        /// <summary>
        ///     Applies the auto lock time change when the value of the selector is changed
        /// </summary>
        private void PrepareChangeLockTime(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            _modifiedSettings[4] = true;
        }

        #endregion

        #region Closing

        /// <summary>
        ///     Applies changes and closes the program
        /// </summary>
        private void OkOnClick(object sender, RoutedEventArgs e)
        {
            Apply();
            Close();
        }

        /// <summary>
        ///     Closes the program
        /// </summary>
        private void CancelOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }


        /// <summary>
        ///     Applies changes
        /// </summary>
        private void ApplyOnClick(object sender, RoutedEventArgs e)
        {
            Apply();
        }

        /// <summary>
        ///     Restores the settings that are changed live
        /// </summary>
        private void Restore()
        {
            if (_modifiedSettings[0])
                ChangeProgramsAccent(ThemeManager.GetAccent(Profile.GetValue("Global", "Accent", "Blue")));

            if (_modifiedSettings[1])
                ChangeProgramsTheme(Profile.GetValue("Global", "Theme", "BaseLight") == "LightBase");

            if (_modifiedSettings[2])
                ChangeProgramsFont(new FontFamily(Profile.GetValue("Global", "MainFont", "Arial")));

            if (_modifiedSettings[3])
                ChangeProgramsFontSize(double.Parse(Profile.GetValue("Global", "MainFontSize", "12")));
        }

        /// <summary>
        ///     Applies changes that are not changed live
        /// </summary>
        private void Apply()
        {
            if (_modifiedSettings[0])
                Profile.SetValue("Global", "Accent", ((Accent) AccentSelector.SelectedItem).Name);

            if (_modifiedSettings[1])
                Profile.SetValue("Global", "Theme", DarkModeToggle.IsChecked == true ? "BaseDark" : "BaseLight");

            if (_modifiedSettings[2])
                Profile.SetValue("Global", "MainFont", (FontFamily) FontSelector.SelectedItem);

            if (_modifiedSettings[3])
                Profile.SetValue("Global", "MainFontSize", FontSizeSelector.Value);

            if (_modifiedSettings[4] && LockTimeSelector.Value != null)
            {
                ChangeLockTime((double) LockTimeSelector.Value);
                Profile.SetValue("Global", "Locktime", LockTimeSelector.Value);
            }

            _modifiedSettings = new bool[NumberOfSettings];
        }

        #endregion
    }
}