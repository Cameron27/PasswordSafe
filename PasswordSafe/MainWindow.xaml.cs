using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using PasswordSafe.Data;
using Newtonsoft.Json;

namespace PasswordSafe
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

            //Will need to change for actual
            string json = File.ReadAllText(@"Resources/Database.json");
            RootObject data = JsonConvert.DeserializeObject<RootObject>(json);

            ConstructFolders(data.Folders);

            ConstructAccountEntries(data.Accounts);
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<MetroWindow>().Any(x => x.Title == "Settings"))
                return; //Check if a settings window is already open

            MetroWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.Left = Left + ActualWidth / 2.0;
            settingsWindow.Top = Top + ActualHeight / 2.0;
            settingsWindow.Show();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region Resize Window

        /// <summary>
        ///     Adjusts grid settings for min and max sizes when overall window size changes
        /// </summary>
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid g = (Grid)sender;
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

        #region Construct Account Entries

        private void ConstructAccountEntries(IEnumerable<Account> accounts)
        {
            AccountList.ItemsSource = accounts;
        }

        #endregion

        #region Highlighting Folders

        private void Folder_MouseEnter(object sender, MouseEventArgs e)
        {
            //TODO Change this back to something that works
            // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            if (sender is Rectangle)
            {
                Rectangle temp = (Rectangle)sender;
                ((Grid)temp.Parent).Children.OfType<Rectangle>()
                    .Last()
                    .SetResourceReference(Shape.FillProperty, "HighlightBrush");
            }
            else if (sender is ToggleButton)
            {
                ToggleButton temp = (ToggleButton)sender;
                ((Grid)temp.Parent).Children.OfType<Rectangle>()
                    .Last()
                    .SetResourceReference(Shape.FillProperty, "HighlightBrush");
            }
            else
            {
                ContentPresenter temp = (ContentPresenter)sender;
                ((Grid)temp.Parent).Children.OfType<Rectangle>()
                    .Last()
                    .SetResourceReference(Shape.FillProperty, "HighlightBrush");
            }
        }

        private void Folder_MouseLeave(object sender, MouseEventArgs e)
        {
            // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            if (sender is Rectangle)
            {
                Rectangle temp = (Rectangle)sender;
                ((Grid)temp.Parent).Children.OfType<Rectangle>()
                    .Last()
                    .SetResourceReference(Shape.FillProperty, "AccentColorBrush");
            }
            else if (sender is ToggleButton)
            {
                ToggleButton temp = (ToggleButton)sender;
                ((Grid)temp.Parent).Children.OfType<Rectangle>()
                    .Last()
                    .SetResourceReference(Shape.FillProperty, "AccentColorBrush");
            }
            else
            {
                ContentPresenter temp = (ContentPresenter)sender;
                ((Grid)temp.Parent).Children.OfType<Rectangle>()
                    .Last()
                    .SetResourceReference(Shape.FillProperty, "AccentColorBrush");
            }
        }

        #endregion

        #region Construct Folders

        private void ConstructFolders(List<Folder> folders)
        {
            Folders.Children.Add(new Label
            {
                Content = "All",
                Padding = new Thickness(10, 5, 5, 5),
                Style = (Style)FindResource("Folder")
            });
            Folders.Children.Add(new Label
            {
                Content = "Prediction",
                Padding = new Thickness(10, 5, 5, 5),
                Style = (Style)FindResource("Folder")
            });
            foreach (Folder folder in folders)
            {
                if (folder.Children.Count == 0)
                    Folders.Children.Add(new Label
                    {
                        Content = folder.Name,
                        Padding = new Thickness(10, 5, 5, 5),
                        Style = (Style)FindResource("Folder")
                    });
                else
                    Folders.Children.Add(MakeDropDownFolder(folder));
            }
        }

        private Expander MakeDropDownFolder(Folder folder)
        {
            Expander output = new Expander
            {
                Header = folder.Name,
                Padding = new Thickness((folder.Path.Count(x => x == '/') - 1) * 10 + 5, 0, 0, 0),
                Style = (Style)FindResource("DropDownFolder")
            };
            StackPanel stackPanel = new StackPanel();
            foreach (Folder childFolder in folder.Children)
            {
                if (childFolder.Children.Count == 0)
                    stackPanel.Children.Add(new Label
                    {
                        Content = childFolder.Name,
                        Padding = new Thickness((childFolder.Path.Count(x => x == '/') - 1) * 10 + 10, 5, 5, 5),
                        Style = (Style)FindResource("Folder")
                    });
                else
                    stackPanel.Children.Add(MakeDropDownFolder(childFolder));
            }
            output.Content = stackPanel;
            return output;
        }

        #endregion
    }
}