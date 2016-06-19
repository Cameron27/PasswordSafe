using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro;
using MahApps.Metro.Controls;

namespace MockupApplication
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
            FontSizeSelector.Text = Application.Current.Resources["MainFontSize"].ToString();

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

        private void DarkModeToggle_Clicked(object sender, RoutedEventArgs e)
        {
            Tuple<AppTheme, Accent> theme = ThemeManager.DetectAppStyle(this);
            ThemeManager.ChangeAppStyle(Application.Current, theme.Item2,
                ThemeManager.GetAppTheme("Base" + (DarkModeToggle.IsChecked == true ? "Dark" : "Light")));
        }

        private void FontSelector_Changed(object sender, SelectionChangedEventArgs e)
        {
            Application.Current.Resources["MainFont"] = FontSelector.SelectedItem as FontFamily;
        }

        private void FontSizeSelector_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            double temp;
            if (double.TryParse(FontSizeSelector.Text, out temp) && temp <= 32 && temp > 0)
            {
                Application.Current.Resources["MainFontSize"] = temp;
            }

        }
    }
}