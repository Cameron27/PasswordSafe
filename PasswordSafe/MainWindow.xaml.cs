using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using PasswordSafe.Data;

namespace PasswordSafe
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static RootObject SafeData;
        private static ObservableCollection<Account> _accountFilter;
        private static bool _needsSaving;
        private static string _openFile;

        public MainWindow(string openFile)
        {
            InitializeComponent();
            Height = SystemParameters.PrimaryScreenHeight * 0.75;
            Width = SystemParameters.PrimaryScreenWidth * 0.75;
            _openFile = openFile;

            if (File.ReadAllText($"Resources/{_openFile}") != "")
            {
                string json = File.ReadAllText($"Resources/{_openFile}");
                SafeData = JsonConvert.DeserializeObject<RootObject>(json);
            }
            else
            {
                SafeData = new RootObject {Folders = new List<Folder>(), Accounts = new List<Account>()};
            }

            ConstructFolders(SafeData.Folders);

            ConstructAccountEntries();
        }

        #region Settings

        /// <summary>
        ///     Opens the settings window
        /// </summary>
        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<MetroWindow>().Any(x => x.Title == "SettingsWindow"))
                return; //Check if a settings window is already open

            MetroWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.Left = Left + ActualWidth / 2.0;
            settingsWindow.Top = Top + ActualHeight / 2.0;
            settingsWindow.Show();
        }

        #endregion

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

        #region Construct Account Entries

        /// <summary>
        ///     Fills AccountList DataGrid with all the accounts
        /// </summary>
        private void ConstructAccountEntries()
        {
            _accountFilter = new ObservableCollection<Account>();
            SafeData.Accounts.ForEach(x => _accountFilter.Add(x));
            AccountList.ItemsSource = _accountFilter;
        }

        #endregion

        #region Controls

        /// <summary>
        ///     Closes the window
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!_needsSaving) return;
            Thread save = new Thread(Save);
            save.Start();
        }

        private void SaveOnCtrlSPress(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control || e.Key != Key.S || !_needsSaving) return;
            Thread save = new Thread(Save);
            save.Start();
        }

        private void Save()
        {
            _needsSaving = false;
            string jsonText = JsonConvert.SerializeObject(SafeData);
            File.WriteAllText($"Resources\\{_openFile}.bak", jsonText);
            File.WriteAllText($"Resources\\{_openFile}", jsonText);
            File.Delete($"Resources\\{_openFile}.bak");
        }

        #endregion

        #region Entry Editor

        /// <summary>
        ///     Creates a new instance of EntryEditorWindow to create a new Account
        /// </summary>
        private void AddEntry_OnClick(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<MetroWindow>().Any(x => x.Title == "EntryEditorWindow"))
                return; //Check if a entry editor window is already open

            Account newAccount = new Account();
            EntryEditorWindow entryEditorWindow = new EntryEditorWindow(true, newAccount)
            {
                Owner = this,
                Left = Left + ActualWidth / 2.0,
                Top = Top + ActualHeight / 2.0
            };
            if (entryEditorWindow.ShowDialog() != true) return;
            _needsSaving = true;
            SafeData.Accounts.Add(entryEditorWindow.AccountBeingEdited);
            ConstructAccountEntries();
        }

        /// <summary>
        ///     Creates a new instance of EntryEditorWindow to edit the currently selected Account on the AccountList DataGrid
        /// </summary>
        private void EditEntry_OnClick(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<MetroWindow>().Any(x => x.Title == "EntryEditorWindow") ||
                AccountList.SelectedItem == null)
                return; //Check if a editor window window is already open or no account is selected

            Account editedAccount = (Account) AccountList.SelectedItem;
            EntryEditorWindow entryEditorWindow = new EntryEditorWindow(false, editedAccount)
            {
                Owner = this,
                Left = Left + ActualWidth / 2.0,
                Top = Top + ActualHeight / 2.0
            };
            if (entryEditorWindow.ShowDialog() != true) return;
            _needsSaving = true;
            SafeData.Accounts[SafeData.Accounts.FindIndex(x => x.Id == entryEditorWindow.AccountBeingEdited.Id)] =
                entryEditorWindow.AccountBeingEdited;
            //Finds the Account in SafeData with a matching ID to the selected account and then sets it to the modified version
            ConstructAccountEntries();
        }

        #endregion

        #region Highlighting Folders

        /// <summary>
        ///     Highlights folder if it is hovered over
        /// </summary>
        private void Folder_MouseEnter(object sender, MouseEventArgs e)
        {
            // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            if (sender is Rectangle)
            {
                Rectangle temp = (Rectangle) sender;
                ((Grid) temp.Parent).Children.OfType<Rectangle>()
                    .Last()
                    .SetResourceReference(Shape.FillProperty, "HighlightBrush");
            }
            else if (sender is ToggleButton)
            {
                ToggleButton temp = (ToggleButton) sender;
                ((Grid) temp.Parent).Children.OfType<Rectangle>()
                    .Last()
                    .SetResourceReference(Shape.FillProperty, "HighlightBrush");
            }
            else
            {
                ContentPresenter temp = (ContentPresenter) sender;
                ((Grid) temp.Parent).Children.OfType<Rectangle>()
                    .Last()
                    .SetResourceReference(Shape.FillProperty, "HighlightBrush");
            }
        }

        /// <summary>
        ///     Unhighlights folder when mouse if removed from it
        /// </summary>
        private void Folder_MouseLeave(object sender, MouseEventArgs e)
        {
            // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
            if (sender is Rectangle)
            {
                Rectangle temp = (Rectangle) sender;
                ((Grid) temp.Parent).Children.OfType<Rectangle>()
                    .Last()
                    .SetResourceReference(Shape.FillProperty, "AccentColorBrush");
            }
            else if (sender is ToggleButton)
            {
                ToggleButton temp = (ToggleButton) sender;
                ((Grid) temp.Parent).Children.OfType<Rectangle>()
                    .Last()
                    .SetResourceReference(Shape.FillProperty, "AccentColorBrush");
            }
            else
            {
                ContentPresenter temp = (ContentPresenter) sender;
                ((Grid) temp.Parent).Children.OfType<Rectangle>()
                    .Last()
                    .SetResourceReference(Shape.FillProperty, "AccentColorBrush");
            }
        }

        #endregion

        #region Construct Folders

        /// <summary>
        ///     Create the folders in the folder menu
        /// </summary>
        /// <param name="folders">The list of folders to use to create the folder menu</param>
        private void ConstructFolders(List<Folder> folders)
        {
            //Two default folders
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

            foreach (Folder folder in folders) //Creates a folder for every folder in the SafeData
            {
                if (folder.Children.Count == 0)
                    Folders.Children.Add(new Label
                    {
                        Content = folder.Name,
                        Padding = new Thickness(10, 5, 5, 5),
                        Style = (Style) FindResource("Folder")
                    });
                else
                    Folders.Children.Add(MakeDropDownFolder(folder));
            }
        }

        /// <summary>
        ///     Creates a expander control from a folder input
        /// </summary>
        /// <param name="folder">Folder to create expander from</param>
        /// <returns>Dropdown folder</returns>
        private Expander MakeDropDownFolder(Folder folder)
        {
            Expander output = new Expander
            {
                Header = folder.Name,
                Padding = new Thickness((folder.Path.Count(x => x == '/') - 1) * 10 + 5, 0, 0, 0),
                //Sets padding based on indentation
                Style = (Style) FindResource("DropDownFolder")
            };
            StackPanel stackPanel = new StackPanel();
            foreach (Folder childFolder in folder.Children)
            {
                if (childFolder.Children.Count == 0)
                    stackPanel.Children.Add(new Label
                    {
                        Content = childFolder.Name,
                        Padding = new Thickness((childFolder.Path.Count(x => x == '/') - 1) * 10 + 10, 5, 5, 5),
                        //Sets padding based on indentation
                        Style = (Style) FindResource("Folder")
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