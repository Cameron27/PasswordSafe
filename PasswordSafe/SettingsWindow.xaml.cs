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

        private void AccentSelector_Changed(object sender, SelectionChangedEventArgs e)
        {
            Accent selectedAccent = AccentSelector.SelectedItem as Accent;
            if (selectedAccent != null)
            {
                Tuple<AppTheme, Accent> theme = ThemeManager.DetectAppStyle(Application.Current);
                ThemeManager.ChangeAppStyle(Application.Current, selectedAccent, theme.Item1);
            }
        }

        private void DarkModeToggle_IsCheckedChanged(object sender, EventArgs e)
        {
            Thread changeAppThemeThread = new Thread(ChangeAppTheme);
            changeAppThemeThread.Start();
        }

        private void FontSelector_Changed(object sender, SelectionChangedEventArgs e)
        {
            Application.Current.Resources["MainFont"] = FontSelector.SelectedItem as FontFamily;
        }

        private void FontSizeSelector_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            if (FontSizeSelector.Value <= 32 && FontSizeSelector.Value > 0)
            {
                Application.Current.Resources["MainFontSize"] = FontSizeSelector.Value;
            }
        }

        private void ChangeAppTheme()
        {
            Tuple<AppTheme, Accent> theme = ThemeManager.DetectAppStyle(this);
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(
                    () =>
                        ThemeManager.ChangeAppStyle(Application.Current, theme.Item2,
                            ThemeManager.GetAppTheme("Base" + (DarkModeToggle.IsChecked == true ? "Dark" : "Light")))));
        }
    }
}