using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro;
using MahApps.Metro.Controls;
using PasswordSafe.GlobalClasses;
using PasswordSafe.GlobalClasses.CustomControls;

namespace PasswordSafe.Windows
{
    /// <summary>
    ///     Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        private Dictionary<string, string[]> _changedSettings = new Dictionary<string, string[]>();

        public SettingsWindow()
        {
            InitializeComponent();

            //Security
            LockOnMinimiseCheck.IsChecked = MainWindow.Profile.GetValue("Security", "LockOnMinimise", "false") == "true";
            AutoLockTimeCheck.IsChecked = MainWindow.Profile.GetValue("Security", "AutoLockTimeBool", "true") == "true";
            AutoLockTimeValue.Value = double.Parse(MainWindow.Profile.GetValue("Security", "AutoLockTimeValue", "5"));
            LimitPasswordCharactersCheck.IsChecked =
                MainWindow.Profile.GetValue("Security", "LimitPasswordCharacters", "true") == "true";
            PasswordLength.Value = double.Parse(MainWindow.Profile.GetValue("Security", "RandomPasswordLength", "24"));
            if (MainWindow.Profile.GetValue("Security", "AutoClearClipboardBool", "true") == "true")
                AutoClearClipboardCheck.IsChecked = true;
            AutoClearClipboardValue.Value =
                double.Parse(MainWindow.Profile.GetValue("Security", "AutoClearClipboardValue", "5"));

            //Appearance
            Tuple<AppTheme, Accent> currentStyle = ThemeManager.DetectAppStyle(Application.Current);
            AccentSelector.SelectedValue = currentStyle.Item2;
            if (currentStyle.Item1 == ThemeManager.GetAppTheme("BaseDark"))
                DarkThemeButton.IsChecked = true;
            else
                LightThemeButton.IsChecked = true;

            FontSelector.SelectedValue = Application.Current.Resources["MainFont"];
            FontSizeSelector.Value = (double?) Application.Current.Resources["MainFontSize"];

            //Advanced
            ExitOnAutoLockCheck.IsChecked = MainWindow.Profile.GetValue("Advanced", "ExitOnAutoLock", "false") == "true";
            AutoSaveCheck.IsChecked = MainWindow.Profile.GetValue("Advanced", "AutoSave", "false") == "true";
            AutoBackupCheck.IsChecked = MainWindow.Profile.GetValue("Advanced", "AutoBackup", "true") == "true";
            DeleteBackupsOnSaveCheck.IsChecked =
                MainWindow.Profile.GetValue("Advanced", "DeleteBackupsOnSave", "false") == "true";
            CopyUrlsToClipboardCheck.IsChecked =
                MainWindow.Profile.GetValue("Advanced", "CopyUrlsToClipboard", "false") == "true";
            DisablePasswordPeakingCheck.IsChecked =
                MainWindow.Profile.GetValue("Advanced", "DisablePasswordPeaking", "false") == "true";

            _changedSettings = new Dictionary<string, string[]>();
        }

        #region Other

        private void ChangeSettingsWindow(object sender, MouseButtonEventArgs e)
        {
            SettingsLabels.Children.Cast<SettingsLabel>().ToList().ForEach(x => x.IsSelected = false);
            SettingsLabel clickedLabel = (SettingsLabel) sender;
            clickedLabel.IsSelected = true;

            //Sets the Zindex of everything
            foreach (object mainRegionChild in MainRegion.Children)
            {
                if (!(mainRegionChild is Grid)) continue;

                Grid settingsGrid = (Grid) mainRegionChild;
                Panel.SetZIndex(settingsGrid, settingsGrid.Name == (string) clickedLabel.Content ? 1 : -1);
            }
        }

        #endregion

        #region Settings Changed

        /// <summary>
        ///     Records the fact that a setting has been changed
        /// </summary>
        /// <param name="settingSection">Section of the setting</param>
        /// <param name="settingName">Name of the setting</param>
        /// <param name="newSettingValue">New value for the setting</param>
        /// <param name="defalutSettingValue">Default value for the setting</param>
        private void LogSettingChange(string settingSection, string settingName, string newSettingValue,
            string defalutSettingValue)
        {
            if (newSettingValue == "")
                if (_changedSettings.ContainsKey(settingName))
                    newSettingValue = _changedSettings[settingName][1];
                else
                    return;

            if (!_changedSettings.ContainsKey(settingName))
                _changedSettings.Add(settingName,
                    new[]
                    {
                        newSettingValue,
                        MainWindow.Profile.GetValue(settingSection, settingName, defalutSettingValue),
                        settingSection
                    });
            else
                _changedSettings[settingName][0] = newSettingValue;
        }

        #region Settings Changed - Security

        private void LockOnMinimiseCheckChanged(object sender, RoutedEventArgs e)
        {
            Debug.Assert(LockOnMinimiseCheck.IsChecked != null, "LockOnMinimiseCheck.IsChecked != null");
            string newValue = (bool) LockOnMinimiseCheck.IsChecked ? "true" : "false";
            LogSettingChange("Security", "LockOnMinimise", newValue, "false");
        }

        private void AutoLockTimeCheckChanged(object sender, RoutedEventArgs e)
        {
            Debug.Assert(AutoLockTimeCheck.IsChecked != null, "AutoLockTimeCheck.IsChecked != null");
            string newValue = (bool) AutoLockTimeCheck.IsChecked ? "true" : "false";
            LogSettingChange("Security", "AutoLockTimeBool", newValue, "true");
        }

        private void AutoLockTimeValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            string newValue = AutoLockTimeValue.Value.ToString();
            LogSettingChange("Security", "AutoLockTimeValue", newValue, "5");
        }

        private void LimitPasswordCharactersCheckChanged(object sender, RoutedEventArgs e)
        {
            Debug.Assert(LimitPasswordCharactersCheck.IsChecked != null,
                "LimitPasswordCharactersCheck.IsChecked != null");
            string newValue = (bool) LimitPasswordCharactersCheck.IsChecked ? "true" : "false";
            LogSettingChange("Security", "LimitPasswordCharacters", newValue, "true");
        }

        private void PasswordLengthChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            string newValue = PasswordLength.Value.ToString();
            LogSettingChange("Security", "RandomPasswordLength", newValue, "24");
        }

        private void ClearClipboardCheckChanged(object sender, RoutedEventArgs e)
        {
            Debug.Assert(AutoClearClipboardCheck.IsChecked != null, "AutoClearClipboardCheck.IsChecked != null");
            string newValue = (bool) AutoClearClipboardCheck.IsChecked ? "true" : "false";
            LogSettingChange("Security", "AutoClearClipboardBool", newValue, "true");
        }

        private void ClearClipboardValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            string newValue = AutoClearClipboardValue.Value.ToString();
            LogSettingChange("Security", "AutoClearClipboardValue", newValue, "10");
        }

        #endregion

        #region Settings Changed - Appearance

        private void AccentSelectorChanged(object sender, SelectionChangedEventArgs e)
        {
            string newValue = ((Accent) AccentSelector.SelectedItem).Name;

            const string settingName = "Accent";
            if (!_changedSettings.ContainsKey(settingName))
                _changedSettings.Add(settingName,
                    new[]
                    {
                        newValue,
                        ThemeManager.DetectAppStyle(Application.Current).Item2.Name,
                        "Appearance"
                    });
            else
                _changedSettings[settingName][0] = newValue;

            ModifySettings.ChangeProgramsAccent((Accent) AccentSelector.SelectedItem);
        }

        private void ThemeButtonChanged(object sender, RoutedEventArgs e)
        {
            Debug.Assert(LightThemeButton.IsChecked != null, "LightThemeButton.IsChecked != null");
            string newValue = (bool) LightThemeButton.IsChecked ? "BaseLight" : "DarkBase";

            const string settingName = "Theme";
            if (!_changedSettings.ContainsKey(settingName))
                _changedSettings.Add(settingName,
                    new[]
                    {
                        newValue,
                        ThemeManager.DetectAppStyle(Application.Current).Item1.Name,
                        "Appearance"
                    });
            else
                _changedSettings[settingName][0] = newValue;

            ModifySettings.ChangeProgramsTheme((bool) LightThemeButton.IsChecked);
        }

        private void FontSelectorChanged(object sender, SelectionChangedEventArgs e)
        {
            string newValue = ((FontFamily) FontSelector.SelectedItem).Source;

            const string settingName = "Font";
            if (!_changedSettings.ContainsKey(settingName))
                _changedSettings.Add(settingName,
                    new[]
                    {
                        newValue,
                        ((FontFamily) Application.Current.Resources["MainFont"]).Source,
                        "Appearance"
                    });
            else
                _changedSettings[settingName][0] = newValue;

            ModifySettings.ChangeProgramsFont((FontFamily) FontSelector.SelectedItem);
        }

        private void FontSizeSelectorChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            string newValue = FontSizeSelector.Value.ToString();

            const string settingName = "FontSize";

            if (FontSizeSelector.Value == null)
                if (_changedSettings.ContainsKey(settingName))
                    newValue = _changedSettings[settingName][1];
                else
                    return;


            if (!_changedSettings.ContainsKey(settingName))
                _changedSettings.Add(settingName,
                    new[]
                    {
                        newValue,
                        ((double) Application.Current.Resources["MainFontSize"]).ToString(CultureInfo.InvariantCulture),
                        "Appearance"
                    });
            else
                _changedSettings[settingName][0] = newValue;

            ModifySettings.ChangeProgramsFontSize(double.Parse(newValue));
        }

        #endregion

        #region Settings Changed - Advanced

        private void ExitOnAutoLockCheckChanged(object sender, RoutedEventArgs e)
        {
            Debug.Assert(ExitOnAutoLockCheck.IsChecked != null, "ExitOnAutoLockCheck.IsChecked != null");
            string newValue = (bool) ExitOnAutoLockCheck.IsChecked ? "true" : "false";
            LogSettingChange("Advanced", "ExitOnAutoLock", newValue, "false");
        }

        private void AutoSaveCheckChanged(object sender, RoutedEventArgs e)
        {
            Debug.Assert(AutoSaveCheck.IsChecked != null, "AutoSaveCheck.IsChecked != null");
            string newValue = (bool) AutoSaveCheck.IsChecked ? "true" : "false";
            LogSettingChange("Advanced", "AutoSave", newValue, "false");
        }

        private void AutoBackupCheckChanged(object sender, RoutedEventArgs e)
        {
            Debug.Assert(AutoBackupCheck.IsChecked != null, "AutoBackupCheck.IsChecked != null");
            string newValue = (bool) AutoBackupCheck.IsChecked ? "true" : "false";
            LogSettingChange("Advanced", "AutoBackup", newValue, "true");
        }

        private void DeleteBackupsOnSaveCheckChanged(object sender, RoutedEventArgs e)
        {
            Debug.Assert(DeleteBackupsOnSaveCheck.IsChecked != null, "DeleteBackupsOnSaveCheck.IsChecked != null");
            string newValue = (bool) DeleteBackupsOnSaveCheck.IsChecked ? "true" : "false";
            LogSettingChange("Advanced", "DeleteBackupsOnSave", newValue, "false");
        }

        private void CopyUrlsToClipboardCheckChanged(object sender, RoutedEventArgs e)
        {
            Debug.Assert(CopyUrlsToClipboardCheck.IsChecked != null, "CopyUrlsToClipboardCheck.IsChecked != null");
            string newValue = (bool) CopyUrlsToClipboardCheck.IsChecked ? "true" : "false";
            LogSettingChange("Advanced", "CopyUrlsToClipboard", newValue, "false");
        }

        private void DisablePasswordPeakingCheckChanged(object sender, RoutedEventArgs e)
        {
            Debug.Assert(DisablePasswordPeakingCheck.IsChecked != null, "DisablePasswordPeakingCheck.IsChecked != null");
            string newValue = (bool) DisablePasswordPeakingCheck.IsChecked ? "true" : "false";
            LogSettingChange("Advanced", "DisablePasswordPeaking", newValue, "false");
        }

        #endregion

        #endregion

        #region Closing

        /// <summary>
        ///     Restores settings before the window closes (intended for if the X button is clicked)
        /// </summary>
        private void MetroWindowClosing(object sender, CancelEventArgs e)
        {
            Restore();
        }

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
            Restore();
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
            if (_changedSettings.ContainsKey("Accent") &&
                (_changedSettings["Accent"][0] != _changedSettings["Accent"][1]))
                ModifySettings.ChangeProgramsAccent(ThemeManager.GetAccent(_changedSettings["Accent"][1]));

            if (_changedSettings.ContainsKey("Theme") && (_changedSettings["Theme"][0] != _changedSettings["Theme"][1]))
                ModifySettings.ChangeProgramsTheme(_changedSettings["Theme"][1] == "BaseLight");

            if (_changedSettings.ContainsKey("Font") && (_changedSettings["Font"][0] != _changedSettings["Font"][1]))
                ModifySettings.ChangeProgramsFont(new FontFamily(_changedSettings["Font"][1]));

            if (_changedSettings.ContainsKey("FontSize") &&
                (_changedSettings["FontSize"][0] != _changedSettings["FontSize"][1]))
                ModifySettings.ChangeProgramsFontSize(double.Parse(_changedSettings["FontSize"][1]));
        }

        /// <summary>
        ///     Applies changes that are not changed live
        /// </summary>
        private void Apply()
        {
            foreach (KeyValuePair<string, string[]> changedSetting in _changedSettings)
            {
                //Skips if value wasn't actually changed
                if (changedSetting.Value[0] == changedSetting.Value[1]) continue;

                MainWindow.Profile.SetValue(changedSetting.Value[2], changedSetting.Key, changedSetting.Value[0]);
            }

            _changedSettings = new Dictionary<string, string[]>();
        }

        #endregion
    }
}