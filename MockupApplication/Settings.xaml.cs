using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro;
using MahApps.Metro.Controls;

namespace MockupApplication
{
    /// <summary>
    ///     Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : MetroWindow
    {
        public Settings()
        {
            InitializeComponent();
            if (ThemeManager.DetectAppStyle(Application.Current).Item1 == ThemeManager.GetAppTheme("BaseDark"))
                DarkModeToggle.IsChecked = true;
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
            ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, ThemeManager.GetAppTheme("Base" + (DarkModeToggle.IsChecked == true ? "Dark" : "Light")));            
        }
    }
}