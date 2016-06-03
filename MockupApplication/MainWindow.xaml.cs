using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Newtonsoft.Json.Linq;

namespace MockupApplication
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Height = SystemParameters.PrimaryScreenHeight * 0.75;
            Width = SystemParameters.PrimaryScreenWidth * 0.75;
            //TODO Learn how to actually use JSON
            const string json = Account.Jsonfile;
            JObject o = JObject.Parse(json);
            ConstructFolders((JObject) o["filesystem"]);
            ConstructAccountEntries((JArray) o["accounts"]);
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<MetroWindow>().Any(x => x.Title == "Settings"))
                return; //Check if a settings window is already open

            MetroWindow settingsWindow = new Settings();
            settingsWindow.Owner = this;
            settingsWindow.Closed += (o, args) => settingsWindow = null;
            settingsWindow.Left = Left + ActualWidth / 2.0;
            settingsWindow.Top = Top + ActualHeight / 2.0;
            settingsWindow.Show();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        #region Resize Window

        /// <summary>
        ///     Adjusts grid settings for min and max sizes when overall window size changes
        /// </summary>
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid g = (Grid) sender;
            double column2MinWidth = g.ColumnDefinitions[0].MinWidth;

            //Adjust the max width of the first column to enforce the min width of the second column
            g.ColumnDefinitions[0].MaxWidth = e.NewSize.Width - column2MinWidth - g.ColumnDefinitions[1].ActualWidth;

            //Adjusts the width of the first column if the second column becomes too small from resizing
            if (g.ActualWidth - (g.ColumnDefinitions[0].ActualWidth + g.ColumnDefinitions[1].ActualWidth) <
                column2MinWidth)
            {
                double newColumn0Width = g.ActualWidth - g.ColumnDefinitions[1].ActualWidth - column2MinWidth;
                g.ColumnDefinitions[0].Width = new GridLength(newColumn0Width);
            }
        }

        #endregion

        #region Highlighting Folders

        private void Folder_MouseEnter(object sender, MouseEventArgs e)
        {
            // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            if (sender is Rectangle)
            {
                Rectangle temp = (Rectangle) sender;
                ((Grid) temp.Parent).Children.OfType<Rectangle>().Last().Fill =
                    (Brush) Application.Current.Resources["HighlightBrush"];
            }
            else if (sender is ToggleButton)
            {
                ToggleButton temp = (ToggleButton) sender;
                ((Grid) temp.Parent).Children.OfType<Rectangle>().Last().Fill =
                    (Brush) Application.Current.Resources["HighlightBrush"];
            }
            else
            {
                ContentPresenter temp = (ContentPresenter) sender;
                ((Grid) temp.Parent).Children.OfType<Rectangle>().Last().Fill =
                    (Brush) Application.Current.Resources["HighlightBrush"];
            }
        }

        private void Folder_MouseLeave(object sender, MouseEventArgs e)
        {
            // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            if (sender is Rectangle)
            {
                Rectangle temp = (Rectangle) sender;
                ((Grid) temp.Parent).Children.OfType<Rectangle>().Last().Fill =
                    (Brush) Application.Current.Resources["AccentColorBrush"];
            }
            else if (sender is ToggleButton)
            {
                ToggleButton temp = (ToggleButton) sender;
                ((Grid) temp.Parent).Children.OfType<Rectangle>().Last().Fill =
                    (Brush) Application.Current.Resources["AccentColorBrush"];
            }
            else
            {
                ContentPresenter temp = (ContentPresenter) sender;
                ((Grid) temp.Parent).Children.OfType<Rectangle>().Last().Fill =
                    (Brush) Application.Current.Resources["AccentColorBrush"];
            }
        }

        #endregion

        #region Construct Account Entries

        private void ConstructAccountEntries(JArray o)
        {
            foreach (JToken account in o)
            {
                AccountNameColumn.Children.Add(new TextBlock
                {
                    Text = (string) account["accountname"],
                    Style = (Style) FindResource("AccountListViewLabel")
                });
                UsernameColumn.Children.Add(new TextBlock
                {
                    Text = (string) account["username"],
                    Style = (Style) FindResource("AccountListViewLabel")
                });
                EmailColumn.Children.Add(new TextBlock
                {
                    Text = (string) account["email"],
                    Style = (Style) FindResource("AccountListViewLabel")
                });
                PasswordColumn.Children.Add(new TextBlock
                {
                    Text = (string) account["password"],
                    Style = (Style) FindResource("AccountListViewLabel")
                });
                UrlColumn.Children.Add(new TextBlock
                {
                    Text = (string) account["url"],
                    Style = (Style) FindResource("AccountListViewLabel")
                });
                NotesColumn.Children.Add(new TextBlock
                {
                    Text = (string) account["notes"],
                    Style = (Style) FindResource("AccountListViewLabel")
                });
            }
        }

        #endregion

        #region Construct Folders

        private void ConstructFolders(JObject o)
        {
            Folders.Children.Add(new Label
            {
                Content = "All",
                Padding = new Thickness(10, 5, 5, 5),
                Style = (Style) FindResource("Folder")
            });
            Folders.Children.Add(new Label
            {
                Content = "Prediction",
                Padding = new Thickness(10, 5, 5, 5),
                Style = (Style) FindResource("Folder")
            });
            foreach (KeyValuePair<string, JToken> pair in o)
            {
                if (!pair.Value.Any())
                    Folders.Children.Add(new Label
                    {
                        Content = pair.Key,
                        Padding = new Thickness(10, 5, 5, 5),
                        Style = (Style) FindResource("Folder")
                    });
                else
                    Folders.Children.Add(MakeDropDownFolder(pair, 1));
            }
        }

        private Expander MakeDropDownFolder(KeyValuePair<string, JToken> pair, int depth)
        {
            Expander output = new Expander
            {
                Header = pair.Key,
                Padding = new Thickness((depth - 1) * 10 + 5, 0, 0, 0),
                Style = (Style) FindResource("DropDownFolder")
            };
            StackPanel stackPanel = new StackPanel();
            foreach (KeyValuePair<string, JToken> pair2 in (JObject) pair.Value)
            {
                if (!pair2.Value.Any())
                    stackPanel.Children.Add(new Label
                    {
                        Content = pair2.Key,
                        Padding = new Thickness(depth * 10 + 10, 5, 5, 5),
                        Style = (Style) FindResource("Folder")
                    });
                else
                    stackPanel.Children.Add(MakeDropDownFolder(pair2, depth + 1));
            }
            output.Content = stackPanel;
            return output;
        }

        #endregion
    }
}