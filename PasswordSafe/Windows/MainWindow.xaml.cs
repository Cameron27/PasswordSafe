using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using AMS.Profile;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using PasswordSafe.CustomControls;
using PasswordSafe.Data;
using PasswordSafe.DialogBoxes;

namespace PasswordSafe.Windows
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
        private static Thread _clearClipboardThread;
        private static Thread _saveThread;
        private static string _folderFilter = "";
        private static readonly Xml Profile = new Xml("config.xml");

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

            //A list of all the column headers and the name of the corresponding binding
            List<Tuple<string, string>> columnsToGenerate = new List<Tuple<string, string>>
            {
                Tuple.Create("Account", "AccountName"),
                Tuple.Create("Username", "Username"),
                Tuple.Create("Email", "Email"),
                Tuple.Create("Password", "Password"),
                Tuple.Create("URL", "Url"),
                Tuple.Create("Notes", "Notes")
            };

            //Genterate all of the columns for AccountList
            for (int i = 0; i < columnsToGenerate.Count; i++)
            {
                ConstructAccountListColumn(columnsToGenerate[i].Item1, columnsToGenerate[i].Item2, i);
            }

            ConstructAccountEntries();
        }

        #region Settings

        /// <summary>
        ///     Opens the settings window
        /// </summary>
        private void OpenSettingsOnClick(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<MetroWindow>().Any(x => x.Title == "SettingsWindow"))
                return; //Check if a settings window is already open

            SettingsWindow settingsWindow = new SettingsWindow {Owner = this};
            settingsWindow.Show();
        }

        #endregion

        #region Resize Window

        /// <summary>
        ///     Adjusts grid settings for min and max sizes when overall window size changes
        /// </summary>
        private void AdjustRegionSizeLimitsOnWindowSizeCharge(object sender, SizeChangedEventArgs e)
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

        #region Global Events

        /// <summary>
        ///     Checks for global key press events
        /// </summary>
        private void GlobalHotkeys(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                //Starts a new thread to safe the safe when ctrl + s is pressed
                if (e.Key == Key.S && _needsSaving)
                {
                    StartSaveThread();
                }
                //Creates a new instance of AccountEditorWindow to create a new account when ctrl + n is pressed
                else if (e.Key == Key.N)
                {
                    NewAccount();
                }
                //Creates a new instance of AccountEditorWindow to edit the currently selected account when ctrl + e is pressed
                else if (e.Key == Key.E)
                {
                    EditAccount();
                }
            }
        }

        /// <summary>
        ///     Checks for the other global key press events casue this is needed for some reason
        /// </summary>
        private void GlobalHotkeys2(object sender, KeyEventArgs e)
        {
            //Deletes the selected account if the delete key is pressed
            if (e.Key == Key.Delete)
                DeleteAccount();
        }

        /// <summary>
        ///     Runs final commands before closing
        /// </summary>
        private void MetroWindowClosing(object sender, CancelEventArgs e)
        {
            //Terminates the _clearClipboardThread if it is running
            if (_clearClipboardThread != null && _clearClipboardThread.IsAlive)
                _clearClipboardThread.Abort();
            Clipboard.SetText("");

            //Checks if the user wants to save
            if (_needsSaving && DialogBox.QuestionDialogBox("Do you want to save before you quit?", true, this))
                Save();
        }

        #endregion

        #region Buttons

        /// <summary>
        ///     Saves the safe
        /// </summary>
        private void SaveOnClick(object sender, RoutedEventArgs e)
        {
            if (!_needsSaving) return;

            StartSaveThread();
        }

        /// <summary>
        ///     Closes the window
        /// </summary>
        private void CloseOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     Creates a new instance of AccountEditorWindow to create a new account when the button is clicked
        /// </summary>
        private void NewAccountOnClick(object sender, RoutedEventArgs e)
        {
            NewAccount();
        }

        /// <summary>
        ///     Creates a new instance of AccountEditorWindow to edit the currently selected account when the button is pressed
        /// </summary>
        private void EditAccountOnClick(object sender, RoutedEventArgs e)
        {
            EditAccount();
        }

        /// <summary>
        ///     Deletes the currently highlighted account when the button is clicked
        /// </summary>
        private void DeleteAccountOnClick(object sender, RoutedEventArgs e)
        {
            DeleteAccount();
        }

        /// <summary>
        ///     Hides and unhides columns when the context menu is checked and unchecked
        /// </summary>
        private void AdjustColumnVisibilityOnClick(object sender, RoutedEventArgs e)
        {
            MenuItem menuItemClicked = (MenuItem) sender;

            List<MenuItem> allMenuItems = new List<MenuItem>();
            allMenuItems.AddRange(((ContextMenu) menuItemClicked.Parent).Items.OfType<MenuItem>());

            if (menuItemClicked.IsChecked)
            {
                AccountList.Columns.Single(x => (string) x.Header == (string) menuItemClicked.Header).Visibility =
                    Visibility.Visible;
                //Changes the settings file
                int indexToChange = allMenuItems.FindIndex(menuItemClicked.Equals);
                char[] charArray = Profile.GetValue("Global", "VisibleColumns", "111111").ToCharArray();
                charArray[indexToChange] = '1';
                Profile.SetValue("Global", "VisibleColumns", new string(charArray));
            }

            else
            {
                //Prevents the last column from being unchecked leaving the whole DataGrid blank
                if (allMenuItems.Count(x => x.IsChecked) != 0)
                {
                    AccountList.Columns.Single(x => (string) x.Header == (string) menuItemClicked.Header).Visibility =
                        Visibility.Collapsed;
                    //Changes the settings file
                    int indexToChange = allMenuItems.FindIndex(menuItemClicked.Equals);
                    char[] charArray = Profile.GetValue("Global", "VisibleColumns", "111111").ToCharArray();
                    charArray[indexToChange] = '0';
                    Profile.SetValue("Global", "VisibleColumns", new string(charArray));
                }
                else
                    menuItemClicked.IsChecked = true;
            }
        }

        private void FilterByFolderOnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2) return;

            DependencyObject parent;

            if (sender is ContentPresenter)
            {
                parent = ((Grid) ((ContentPresenter) sender).Parent).TemplatedParent;
            }
            else
            {
                parent = ((Grid) ((Rectangle) sender).Parent).TemplatedParent;
            }


            if (parent is FolderLabel)
            {
                if (((FolderLabel) parent).Path == "All")
                    _folderFilter = "";
                else
                    _folderFilter = ((FolderLabel) parent).Path;
            }
            else if (parent is FolderExpander)
                _folderFilter = ((FolderExpander) parent).PathPart;
            ConstructAccountEntries();
        }

        #endregion

        #region Highlighting Folders

        /// <summary>
        ///     Highlights folder if it is hovered over
        /// </summary>
        private void HighlightFolderWhenMouseEnters(object sender, MouseEventArgs e)
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
        private void UnhighlightFolderWhenMouseLeaves(object sender, MouseEventArgs e)
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
        private void ConstructFolders(IEnumerable<Folder> folders)
        {
            //Two default folders
            Folders.Children.Add(new FolderLabel
            {
                Path = "All",
                Content = "All",
                Padding = new Thickness(10, 5, 5, 5),
                Style = (Style) FindResource("Folder")
            });

            foreach (Folder folder in folders) //Creates a folder for every folder in the SafeData
            {
                if (folder.Children.Count == 0)
                    Folders.Children.Add(new FolderLabel
                    {
                        Path = $"/{folder.Name}",
                        Content = folder.Name,
                        Padding = new Thickness(10, 5, 5, 5),
                        Style = (Style) FindResource("Folder")
                    });
                else
                    Folders.Children.Add(MakeDropDownFolder(folder, $"/{folder.Name}"));
            }
        }

        /// <summary>
        ///     Creates a expander control from a folder input
        /// </summary>
        /// <param name="folder">Folder to create expander from</param>
        /// <param name="currentPath">The current path of the folder</param>
        /// <returns>Dropdown folder</returns>
        private Expander MakeDropDownFolder(Folder folder, string currentPath)
        {
            FolderExpander output = new FolderExpander
            {
                PathPart = currentPath,
                Header = folder.Name,
                //Sets padding based on indentation
                Padding = new Thickness((folder.Path.Count(x => x == '/') - 1) * 10 + 5, 0, 0, 0),
                Style = (Style) FindResource("DropDownFolder")
            };

            StackPanel stackPanel = new StackPanel();
            foreach (Folder childFolder in folder.Children)
            {
                if (childFolder.Children.Count == 0)
                    stackPanel.Children.Add(new FolderLabel
                    {
                        Path = $"{currentPath}/{childFolder.Name}",
                        Content = childFolder.Name,
                        Padding = new Thickness((childFolder.Path.Count(x => x == '/') - 1) * 10 + 10, 5, 5, 5),
                        //Sets padding based on indentation
                        Style = (Style) FindResource("Folder")
                    });
                else
                    stackPanel.Children.Add(MakeDropDownFolder(childFolder, $"{currentPath}/{childFolder.Name}"));
            }
            output.Content = stackPanel;
            return output;
        }

        #endregion

        #region Account Modification

        /// <summary>
        ///     Creates a new instance of AccountEditorWindow to create a new account
        /// </summary>
        private void NewAccount()
        {
            if (Application.Current.Windows.OfType<MetroWindow>().Any(x => x.Title == "AccountEditorWindow"))
                return; //Check if a account editor window is already open

            Account newAccount = new Account();
            AccountEditorWindow accountEditorWindow = new AccountEditorWindow(true, newAccount) {Owner = this};
            if (accountEditorWindow.ShowDialog() != true) return;
            _needsSaving = true;
            SafeData.Accounts.Add(accountEditorWindow.AccountBeingEdited);
            ConstructAccountEntries();
        }

        /// <summary>
        ///     Creates a new instance of AccountEditorWindow to edit the currently selected account
        /// </summary>
        private void EditAccount()
        {
            if (Application.Current.Windows.OfType<MetroWindow>().Any(x => x.Title == "AccountEditorWindow") ||
                AccountList.SelectedItem == null)
                return; //Check if a editor window window is already open or no account is selected

            Account editedAccount = (Account) AccountList.SelectedItem;
            AccountEditorWindow accountEditorWindow = new AccountEditorWindow(false, editedAccount) {Owner = this};
            if (accountEditorWindow.ShowDialog() != true) return;
            _needsSaving = true;
            SafeData.Accounts[SafeData.Accounts.FindIndex(x => x.Id == accountEditorWindow.AccountBeingEdited.Id)] =
                accountEditorWindow.AccountBeingEdited;
            //Finds the Account in SafeData with a matching ID to the selected account and then sets it to the modified version
            ConstructAccountEntries();
        }

        /// <summary>
        ///     Deletes the currently highlighted account(s)
        /// </summary>
        private void DeleteAccount()
        {
            if (AccountList.SelectedItems.Count == 0) return;
            //Asks the user if they are sure they want to delete the account
            if (
                DialogBox.QuestionDialogBox(
                    $"Are you sure you want to delete {(AccountList.SelectedItems.Count == 0 ? "this account" : "these accounts")}?",
                    false, this))
            {
                IList accountsToDelete = AccountList.SelectedItems;
                foreach (Account account in accountsToDelete)
                {
                    SafeData.Accounts.Remove(SafeData.Accounts.Find(x => x.Id == account.Id));
                }
                ConstructAccountEntries();
                _needsSaving = true;
            }
        }

        #endregion

        #region Construct DataGrid

        /// <summary>
        ///     Fills AccountList DataGrid with all the accounts
        /// </summary>
        private void ConstructAccountEntries()
        {
            _accountFilter = new ObservableCollection<Account>();
            SafeData.Accounts.Where(x => x.Path.StartsWith(_folderFilter)).ToList().ForEach(x => _accountFilter.Add(x));
            AccountList.ItemsSource = _accountFilter;
        }

        /// <summary>
        ///     Create a column for the AccountList datagrid
        /// </summary>
        /// <param name="header">Columns heading</param>
        /// <param name="binding">Columns binding</param>
        /// <param name="index">The index of the column</param>
        private void ConstructAccountListColumn(string header, string binding, int index)
        {
            DataTemplate dataTemplate = new DataTemplate {DataType = typeof(string)};

            //Containment for the background and lable
            FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));

            //Used to provide something to click to trigger editing
            FrameworkElementFactory background = new FrameworkElementFactory(typeof(Rectangle));
            background.SetValue(StyleProperty, FindResource("CellBackground"));
            grid.AppendChild(background);

            //The main display
            if (binding == "Password")
            {
                FrameworkElementFactory content = new FrameworkElementFactory(typeof(PasswordTextBlock));
                content.SetBinding(PasswordTextBlock.PasswordProperty, new Binding(binding));
                content.SetValue(StyleProperty, FindResource("PasswordCellTextBlock"));
                grid.AppendChild(content);
            }
            else
            {
                FrameworkElementFactory content = new FrameworkElementFactory(typeof(TextBlock));
                content.SetBinding(TextBlock.TextProperty, new Binding(binding));
                content.SetValue(StyleProperty, FindResource("CellTextBlock"));
                if (binding == "Url")
                    content.SetValue(NameProperty, "URL");
                grid.AppendChild(content);
            }

            dataTemplate.VisualTree = grid;

            bool isHidden = Profile.GetValue("Global", "VisibleColumns", "111111")[index] == '0';
            DataGridTemplateColumn column = new DataGridTemplateColumn
            {
                Header = header,
                SortMemberPath = binding,
                CellTemplate = dataTemplate,
                HeaderStyle = (Style) FindResource("ColumnHeader"),
                Visibility = isHidden ? Visibility.Collapsed : Visibility.Visible
            };
            AccountList.Columns.Add(column);

            //Unchecks hidden columns

            if (isHidden)
            {
                ContextMenu contextMenu = (ContextMenu) FindResource("ColumnHeaderContextMenu");
                ((MenuItem) contextMenu.Items[index]).IsChecked = false;
            }
        }

        /// <summary>
        ///     Creates a new instance of AccountEditorWindow to edit an account when it is double clicked
        /// </summary>
        private void EditAccountOnDoubleLeftClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                EditAccount();
        }

        /// <summary>
        ///     Copies the content of a cell to the clipboard
        /// </summary>
        private void CopyCellOnClick(object sender, RoutedEventArgs e)
        {
            CopyCell((TextBlock) ((ContextMenu) ((MenuItem) sender).Parent).PlacementTarget);
        }

        /// <summary>
        ///     Copies the content of a cell to the clipboard when the label is double clicked
        /// </summary>
        private void CopyCellOnLeftClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (((TextBlock)sender).Name == "URL")
                    System.Diagnostics.Process.Start(((TextBlock)sender).Text);
                else
                    CopyCell((TextBlock) sender);
            }
        }

        /// <summary>
        ///     Copies the content of the cell to the clipboard
        /// </summary>
        /// <param name="cell">The cell to copy the data from</param>
        private void CopyCell(TextBlock cell)
        {
            if (cell is PasswordTextBlock)
                Clipboard.SetText(((PasswordTextBlock)cell).Password);
            else
                Clipboard.SetText(cell.Text);

            //Terminates any currently running ClearClipboard threads
            if (_clearClipboardThread != null && _clearClipboardThread.IsAlive)
                _clearClipboardThread.Abort();

            _clearClipboardThread = new Thread(ClearClipboard);
            _clearClipboardThread.Start();
        }

        /// <summary>
        ///     Clears the clipboard after 10 seconds
        /// </summary>
        private void ClearClipboard()
        {
            for (int i = 10; i > 0; i--)
            {
                int secondsLeft = i;
                Application.Current.Dispatcher.Invoke(
                    () =>
                        MessageBox.Content =
                            $"Field copied to clipboard. {secondsLeft} seconds till clipboard is cleared");
                Thread.Sleep(1000);
            }
            Application.Current.Dispatcher.Invoke(() => Clipboard.SetText(""));
            //This line of code can throw a System.Runtime.InteropServices.COMException if the user happens to be pasting at the time of this code being run
            Application.Current.Dispatcher.Invoke(() => MessageBox.Content = "");
        }

        #endregion

        #region Saving

        /// <summary>
        ///     Starts a new thread to save the safe
        /// </summary>
        private void StartSaveThread()
        {
            if (_saveThread != null && _clearClipboardThread.IsAlive)
                _saveThread.Abort();

            _saveThread = new Thread(Save);
            _saveThread.Start();
        }

        /// <summary>
        ///     Saves the safe
        /// </summary>
        private void Save()
        {
            _needsSaving = false;
            string jsonText = JsonConvert.SerializeObject(SafeData);
            File.WriteAllText($"Resources\\{_openFile}.bak", jsonText);
            File.WriteAllText($"Resources\\{_openFile}", jsonText);
            File.Delete($"Resources\\{_openFile}.bak");
            Application.Current.Dispatcher.Invoke(() => MessageBox.Content = "Safe Saved");
        }

        #endregion
    }
}