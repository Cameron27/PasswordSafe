using System;
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
        private static Thread _clearClipboardThread;
        private static Thread _saveThread;

        public MainWindow(string openFile)
        {
            InitializeComponent();
            Height = SystemParameters.PrimaryScreenHeight*0.75;
            Width = SystemParameters.PrimaryScreenWidth*0.75;
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
            foreach (Tuple<string, string> tuple in columnsToGenerate)
            {
                ConstructAccountListColumn(tuple.Item1, tuple.Item2);
            }

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
            settingsWindow.Left = Left + ActualWidth/2.0;
            settingsWindow.Top = Top + ActualHeight/2.0;
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

        /// <summary>
        ///     Starts a new thread to safe the safe
        /// </summary>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!_needsSaving) return;

            StartSaveThread();
        }

        /// <summary>
        ///     Checks for global key press events
        /// </summary>
        private void MetroWindow_KeyDown(object sender, KeyEventArgs e)
        {
            //Starts a new thread to safe the safe when ctrl + s is pressed
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S && _needsSaving)
            {
                StartSaveThread();
            }

        }

        /// <summary>
        ///     Checks for the other global key press events casue this is needed for some reason
        /// </summary>
        private void MetroWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Deletes the selected account if the delete key is pressed
            if (e.Key == Key.Delete)
                DeleteEntry();
        }

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

        #region Entry Modification

        /// <summary>
        ///     Creates a new instance of EntryEditorWindow to create a new Account
        /// </summary>
        private void AddEntry_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<MetroWindow>().Any(x => x.Title == "EntryEditorWindow"))
                return; //Check if a entry editor window is already open

            Account newAccount = new Account();
            EntryEditorWindow entryEditorWindow = new EntryEditorWindow(true, newAccount)
            {
                Owner = this,
                Left = Left + ActualWidth/2.0,
                Top = Top + ActualHeight/2.0
            };
            if (entryEditorWindow.ShowDialog() != true) return;
            _needsSaving = true;
            SafeData.Accounts.Add(entryEditorWindow.AccountBeingEdited);
            ConstructAccountEntries();
        }

        /// <summary>
        ///     Creates a new instance of EntryEditorWindow to edit the currently selected Account on the AccountList DataGrid
        /// </summary>
        private void EditEntry_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<MetroWindow>().Any(x => x.Title == "EntryEditorWindow") ||
                AccountList.SelectedItem == null)
                return; //Check if a editor window window is already open or no account is selected

            Account editedAccount = (Account) AccountList.SelectedItem;
            EntryEditorWindow entryEditorWindow = new EntryEditorWindow(false, editedAccount)
            {
                Owner = this,
                Left = Left + ActualWidth/2.0,
                Top = Top + ActualHeight/2.0
            };
            if (entryEditorWindow.ShowDialog() != true) return;
            _needsSaving = true;
            SafeData.Accounts[SafeData.Accounts.FindIndex(x => x.Id == entryEditorWindow.AccountBeingEdited.Id)] =
                entryEditorWindow.AccountBeingEdited;
            //Finds the Account in SafeData with a matching ID to the selected account and then sets it to the modified version
            ConstructAccountEntries();
        }

        /// <summary>
        ///     Deletes the currently highlighted account
        /// </summary>
        private void DeleteEntry_Click(object sender, RoutedEventArgs e)
        {
            DeleteEntry();
        }

        /// <summary>
        ///     Deletes the currently highlighted account
        /// </summary>
        private void DeleteEntry()
        {
            //Asks the user if they are sure they want to delete the account
            QuestionDialogBox deleteAccountDialogBox =
                new QuestionDialogBox("Are you sure you want to delete this account?", false)
                {
                    Owner = this,
                    Left = Left + ActualWidth/2.0,
                    Top = Top + ActualHeight/2.0
                };

            if (deleteAccountDialogBox.ShowDialog() == true)
            {
                Account editedAccount = (Account) AccountList.SelectedItem;
                SafeData.Accounts.Remove(SafeData.Accounts.Find(x => x.Id == editedAccount.Id));
                ConstructAccountEntries();
                _needsSaving = true;
            }
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
                Padding = new Thickness((folder.Path.Count(x => x == '/') - 1)*10 + 5, 0, 0, 0),
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
                        Padding = new Thickness((childFolder.Path.Count(x => x == '/') - 1)*10 + 10, 5, 5, 5),
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

        #region DataGrid

        /// <summary>
        ///     Create a column for the AccountList datagrid
        /// </summary>
        /// <param name="header">Columns heading</param>
        /// <param name="binding">Columns binding</param>
        private void ConstructAccountListColumn(string header, string binding)
        {
            DataTemplate dataTemplate = new DataTemplate {DataType = typeof(string)};
            FrameworkElementFactory content = new FrameworkElementFactory(typeof(TextBlock));
            content.SetBinding(TextBlock.TextProperty, new Binding(binding));
            content.SetValue(StyleProperty, FindResource("CellTextBlock"));
            content.AddHandler(MouseDownEvent, new MouseButtonEventHandler(DataGridCellLabel_MouseDown));
            dataTemplate.VisualTree = content;

            AccountList.Columns.Add(new DataGridTemplateColumn
            {
                Header = header,
                SortMemberPath = binding,
                CellTemplate = dataTemplate
            });
        }

        /// <summary>
        ///     Copies the content of the clicked cell to the clipboard
        /// </summary>
        private void DataGridCellLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Clipboard.SetText(((TextBlock) sender).Text);

                //Terminates any currently running ClearClipboard threads
                if (_clearClipboardThread != null && _clearClipboardThread.IsAlive)
                    _clearClipboardThread.Abort();

                _clearClipboardThread = new Thread(ClearClipboard);
                _clearClipboardThread.Start();
            }
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

        /// <summary>
        ///     Runs final commands before closing
        /// </summary>
        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            //Terminates the _clearClipboardThread if it is running
            if (_clearClipboardThread != null && _clearClipboardThread.IsAlive)
                _clearClipboardThread.Abort();
            Clipboard.SetText("");

            //Checks if the user wants to save
            if (_needsSaving)
            {
                QuestionDialogBox saveBeforeQuitDialogBox = new QuestionDialogBox(
                    "Do you want to save before you quit?", true)
                {
                    Owner = this,
                    Left = Left + ActualWidth/2.0,
                    Top = Top + ActualHeight/2.0
                };

                if (saveBeforeQuitDialogBox.ShowDialog() == true)
                {
                    Save();
                }
            }
        }

        #endregion
    }
}