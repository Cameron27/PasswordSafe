﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using AMS.Profile;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using PasswordSafe.DialogBoxes;
using PasswordSafe.GlobalClasses;
using PasswordSafe.GlobalClasses.CustomControls;
using PasswordSafe.GlobalClasses.Data;
using Path = System.Windows.Shapes.Path;

namespace PasswordSafe.Windows
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static RootObject SafeData;
        public static double TimeToLock;
        private readonly Xml _profile = new Xml("config.xml");
        private CollectionViewSource _accountsCollectionViewSource;
        private ICollectionView _accountsICollectionView;
        private ObservableCollection<Account> _accountsObservableCollection;
        private Thread _clearClipboardThread;
        private Predicate<object> _filter;
        private string _folderFilter = "";
        private Thread _idleDetectionThread;
        private bool _needsSaving;
        private string _openFile;
        private bool _preventFolderDropOntoFolderArea;
        private Thread _saveThread;

        public MainWindow(string openFile)
        {
            InitializeComponent();
            Height = SystemParameters.PrimaryScreenHeight * 0.75;
            Width = SystemParameters.PrimaryScreenWidth * 0.75;
            _openFile = openFile;
            TimeToLock = double.Parse(_profile.GetValue("Global", "Locktime", "5"));

            //Creates a thread that will check if the user is idle, this has to start first otherwise the program will crash if there is an error loading the safe
            _idleDetectionThread = new Thread(IdleDetectorThread);
            _idleDetectionThread.Start();

            string contentOfFile = "";

            try
            {
                contentOfFile = File.ReadAllText($"Resources/{_openFile}");
            }
            catch (IOException)
            {
                DialogBox.MessageDialogBox("There was an error trying to load that safe.", null);
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }

            if (contentOfFile != "")
                try
                {
                    SafeData = JsonConvert.DeserializeObject<RootObject>(contentOfFile);
                }
                catch (JsonReaderException)
                {
                    DialogBox.MessageDialogBox("That safe is not a valid file format.", null);
                    LoginWindow loginWindow = new LoginWindow();
                    loginWindow.Show();
                    Close();
                }
            else
                SafeData = new RootObject {Folders = new List<Folder>(), Accounts = new List<Account>()};

            DateTime start = DateTime.Now;
            ConstructFolders(SafeData.Folders);
            Debug.WriteLine(DateTime.Now - start);

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
                ConstructAccountListColumn(columnsToGenerate[i].Item1, columnsToGenerate[i].Item2, i);

            ConstructAccountEntries();

            AccountList.SelectedItem = null; //Rows can be randomly selected on startup
        }

        #region Settings

        /// <summary>
        ///     Opens the settings window
        /// </summary>
        private void OpenSettingsOnClick(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<SettingsWindow>().Any())
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

        #region Search Bar

        /// <summary>
        ///     Changes the filter based on what is in the search box when enter is pressed
        /// </summary>
        private void FilterDataBySearchOnEnterPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                FilterDataGrid();
        }

        /// <summary>
        ///     Clears the search box and filters the DataGrid
        /// </summary>
        private void ClearSearchOnClick(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            FilterDataGrid();
        }

        #endregion

        #region Locking Safe

        /// <summary>
        ///     Checks every 1 second if the user has been idle for a set amount of time and closes the window if they have
        /// </summary>
        private void IdleDetectorThread()
        {
            while (true)
            {
                Thread.Sleep(1000);

                IdleTimeInfo idleTime = IdleTimeDetector.GetIdleTimeInfo();

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if ((idleTime.IdleTime.TotalMinutes < TimeToLock) || (TimeToLock == 0)) continue;
                Dispatcher.Invoke(LockSafe);
                //Stops thread
                return;
            }
        }

        /// <summary>
        ///     Locks the safe
        /// </summary>
        private void LockSafe()
        {
            //Closes all other windows
            foreach (Window window in Application.Current.Windows)
                if (!(window is MainWindow))
                    window.Close();

            ResizeMode = ResizeMode.NoResize;
            RightWindowCommands.Visibility = Visibility.Hidden;

            RibbonFade();
            CreateBackground();
            CreateLock();
            CreatePasswordBox();
        }

        /// <summary>
        ///     Fades the biddon from its current color to gray
        /// </summary>
        private void RibbonFade()
        {
            //Creates the brush for the ribbon that will be animated
            SolidColorBrush ribbonBrushAnimation = new SolidColorBrush
            {
                Color = ((SolidColorBrush) FindResource("AccentColorBrush")).Color
            };

            WindowTitleBrush = ribbonBrushAnimation;
            NonActiveWindowTitleBrush = ribbonBrushAnimation;
            GlowBrush = ribbonBrushAnimation;

            ColorAnimation ribbonColorAnimation = new ColorAnimation
            {
                To = Brushes.LightGray.Color,
                Duration = TimeSpan.FromSeconds(1)
            };

            ribbonBrushAnimation.BeginAnimation(SolidColorBrush.ColorProperty, ribbonColorAnimation);
        }

        /// <summary>
        ///     Creates a gray background that fades in
        /// </summary>
        private void CreateBackground()
        {
            //Create background
            Rectangle lockBackground = new Rectangle
            {
                Fill = Brushes.LightGray,
                Height = WindowGrid.ActualHeight,
                Width = WindowGrid.ActualWidth,
                Opacity = 0
            };
            Grid.SetColumn(lockBackground, 0);
            Grid.SetColumnSpan(lockBackground, 3);
            Grid.SetRow(lockBackground, 0);
            Grid.SetRowSpan(lockBackground, 4);

            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromSeconds(1)
            };
            lockBackground.BeginAnimation(OpacityProperty, fadeInAnimation);

            WindowGrid.Children.Add(lockBackground);
        }

        /// <summary>
        ///     Creates a lock picture and plays its animation
        /// </summary>
        private void CreateLock()
        {
            //Checks how much the lock has to be scaled by
            double scale;
            if (WindowGrid.ActualWidth * (650D / 480D) > WindowGrid.ActualHeight)
                scale = WindowGrid.ActualHeight / 1300D;
            else
                scale = WindowGrid.ActualWidth / 960D;

            List<Path> lockParts = new List<Path>();

            Tuple<string, SolidColorBrush>[] pathInformationArray =
            {
                Tuple.Create(
                    "M 0 330 L 0 600 C 0 650 0 650 50 650 L 430 650 C 480 650 480 650 480 600 L 480 330 C 480 280 480 280 430 280 L 50 280 C 0 280 0 280 0 330 Z",
                    Brushes.DarkGray), //Main Part
                Tuple.Create(
                    "M 40 230 L 40 200 A 200 200 180 0 1 440 200 L 440 280 L 380 280 L 380 200 A 140 140 180 0 0 100 200 L 100 230 Z",
                    Brushes.DarkGray), //Top lock bit
                Tuple.Create(
                    "M 180 400 A 60 60 180 0 1 300 400 L 180 400 A 60 60 59.4897626 0 0 209.5384615 451.6923077 L 180 560 L 300 560 L 270.4615385 451.6923077 A 60 60 59.4897626 0 0 300 400",
                    Brushes.Black) //Key hole
            };

            foreach (Tuple<string, SolidColorBrush> pathInformation in pathInformationArray)
            {
                Path temp = CreateLockPart(pathInformation.Item1, pathInformation.Item2, scale);
                lockParts.Add(temp);
                WindowGrid.Children.Add(temp);
            }

            //Animate locking the lock
            ThicknessAnimation moveAnimation = new ThicknessAnimation
            {
                To =
                    new Thickness(WindowGrid.ActualWidth / 2D - 240,
                        WindowGrid.ActualHeight * (5D / 14D) - 325 + 55 * scale, 0, 0),
                Duration = TimeSpan.FromSeconds(1),
                BeginTime = TimeSpan.FromSeconds(3)
            };

            lockParts[1].BeginAnimation(MarginProperty, moveAnimation);
        }

        /// <summary>
        ///     Creates a path with the specified data
        /// </summary>
        /// <param name="pathData">The data for the path</param>
        /// <param name="color">The fill color of the path</param>
        /// <param name="scale">The amount to scale the path, base size is 480x650</param>
        /// <returns>Path based on the data given</returns>
        private Path CreateLockPart(string pathData, Brush color, double scale)
        {
            TransformGroup tg = new TransformGroup();
            ScaleTransform st = new ScaleTransform(scale * 4, scale * 4);
            tg.Children.Add(st);

            //Creates path
            Path path = new Path
            {
                Fill = color,
                Width = 480,
                Height = 650,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Data =
                    (Geometry)
                    TypeDescriptor.GetConverter(typeof(Geometry))
                        .ConvertFrom(
                            pathData),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = tg,
                Margin =
                    new Thickness(WindowGrid.ActualWidth / 2D - 240, WindowGrid.ActualHeight * (5D / 14D) - 325, 0, 0),
                Opacity = 0
            };

            Grid.SetColumn(path, 0);
            Grid.SetColumnSpan(path, 3);
            Grid.SetRow(path, 0);
            Grid.SetRowSpan(path, 4);

            DoubleAnimation scaleAnimation = new DoubleAnimation
            {
                To = scale,
                Duration = TimeSpan.FromSeconds(2),
                BeginTime = TimeSpan.FromSeconds(1)
            };

            st.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            st.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);

            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromSeconds(2),
                BeginTime = TimeSpan.FromSeconds(1)
            };
            path.BeginAnimation(OpacityProperty, fadeInAnimation);

            return path;
        }

        /// <summary>
        ///     Creates the password input box to unlock the safe
        /// </summary>
        private void CreatePasswordBox()
        {
            PasswordBox passwordBox = new PasswordBox
            {
                Height = 25,
                Width = 280,
                Margin = new Thickness(WindowGrid.ActualWidth / 2D - 140, WindowGrid.ActualHeight * (18D / 28D), 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Opacity = 0
            };

            passwordBox.KeyDown += UnlockOnEnterPress;

            Grid.SetColumn(passwordBox, 0);
            Grid.SetColumnSpan(passwordBox, 3);
            Grid.SetRow(passwordBox, 0);
            Grid.SetRowSpan(passwordBox, 4);

            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromSeconds(1),
                BeginTime = TimeSpan.FromSeconds(4)
            };

            passwordBox.BeginAnimation(OpacityProperty, opacityAnimation);

            WindowGrid.Children.Add(passwordBox);

            passwordBox.Focus();
        }

        /// <summary>
        ///     Unlocks the safe when enter is pressed TODO Make this check the password
        /// </summary>
        private void UnlockOnEnterPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                UnlockSafe();
        }

        /// <summary>
        ///     Unlockes the safe
        /// </summary>
        private void UnlockSafe()
        {
            UIElementCollection childrenOfWindowGrid = WindowGrid.Children;

            //Makes background fade out
            Rectangle lockBackground = (Rectangle) childrenOfWindowGrid[childrenOfWindowGrid.Count - 5];

            DoubleAnimation backgroundFadeoutAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(1),
                BeginTime = TimeSpan.FromSeconds(3)
            };
            lockBackground.BeginAnimation(OpacityProperty, backgroundFadeoutAnimation);

            //Makes lock zoom out again
            double scale;
            if (WindowGrid.ActualWidth * (650D / 480D) > WindowGrid.ActualHeight)
                scale = WindowGrid.ActualHeight / 1300D;
            else
                scale = WindowGrid.ActualWidth / 960D;

            DoubleAnimation scaleAnimation = new DoubleAnimation
            {
                To = scale * 4,
                Duration = TimeSpan.FromSeconds(2),
                BeginTime = TimeSpan.FromSeconds(1)
            };

            DoubleAnimation lockFadeoutAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(2),
                BeginTime = TimeSpan.FromSeconds(1)
            };

            for (int i = childrenOfWindowGrid.Count - 4; i < childrenOfWindowGrid.Count - 1; i++)
            {
                Path lockPart = (Path) childrenOfWindowGrid[i];

                lockPart.BeginAnimation(OpacityProperty, lockFadeoutAnimation);

                ScaleTransform st = (ScaleTransform) ((TransformGroup) lockPart.RenderTransform).Children[0];
                st.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                st.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            }

            //Unlocks the lock
            ThicknessAnimation moveAnimation = new ThicknessAnimation
            {
                To = new Thickness(WindowGrid.ActualWidth / 2D - 240, WindowGrid.ActualHeight * (5D / 14D) - 325, 0, 0),
                Duration = TimeSpan.FromSeconds(1)
            };

            childrenOfWindowGrid[childrenOfWindowGrid.Count - 3].BeginAnimation(MarginProperty, moveAnimation);

            //Fadeout password box
            DoubleAnimation passwordboxFadeoutAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(1)
            };

            childrenOfWindowGrid[childrenOfWindowGrid.Count - 1].BeginAnimation(OpacityProperty,
                passwordboxFadeoutAnimation);

            //Fade the ribbon back to its normal color
            SolidColorBrush ribbonBrush = new SolidColorBrush
            {
                Color = Brushes.LightGray.Color
            };

            WindowTitleBrush = ribbonBrush;
            NonActiveWindowTitleBrush = ribbonBrush;
            GlowBrush = ribbonBrush;

            ColorAnimation ribbonColorAnimation = new ColorAnimation
            {
                To = ((SolidColorBrush) FindResource("AccentColorBrush")).Color,
                Duration = TimeSpan.FromSeconds(1),
                BeginTime = TimeSpan.FromSeconds(3)
            };

            ribbonBrush.BeginAnimation(SolidColorBrush.ColorProperty, ribbonColorAnimation);

            //Changes window things back to normal
            ResizeMode = ResizeMode.CanResizeWithGrip;
            RightWindowCommands.Visibility = Visibility.Visible;

            Thread delayFinalUnlockSafeChanges = new Thread(DelayFinalUnlockSafeChanges);
            delayFinalUnlockSafeChanges.Start();
        }

        /// <summary>
        ///     Waits 4 seconds before running the final steps to unlock the safe
        /// </summary>
        private void DelayFinalUnlockSafeChanges()
        {
            Thread.Sleep(4000);
            Dispatcher.Invoke(FinalUnlockSafeChanges);
        }

        /// <summary>
        ///     Runs the final steps to unlock the safe
        /// </summary>
        private void FinalUnlockSafeChanges()
        {
            SetResourceReference(WindowTitleBrushProperty, "AccentColorBrush");
            SetResourceReference(NonActiveWindowTitleBrushProperty, "AccentColorBrush");
            SetResourceReference(GlowBrushProperty, "AccentColorBrush");

            //Removes the lock elements
            int count = WindowGrid.Children.Count;
            for (int i = count - 5; i < count; i++)
                WindowGrid.Children.RemoveAt(count - 5);

            //Restarts the lock thread
            _idleDetectionThread = new Thread(IdleDetectorThread);
            _idleDetectionThread.Start();
        }

        #endregion region

        #region Global Events

        /// <summary>
        ///     Checks for global key press events
        /// </summary>
        private void GlobalHotkeys(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                switch (e.Key)
                {
                    case Key.S:
                        SaveAs();
                        break;
                }

            else if (Keyboard.Modifiers == ModifierKeys.Control)
                switch (e.Key)
                {
                    case Key.N:
                        NewSafe();
                        break;
                    case Key.O:
                        LoginWindow loginWindow = new LoginWindow();
                        loginWindow.Show();
                        Close();
                        break;
                    case Key.S:
                        StartSaveThread();
                        break;
                    case Key.L:
                        LockSafe();
                        _idleDetectionThread.Abort();
                        break;
                    case Key.X:
                        Close();
                        break;
                    case Key.F:
                        CreateNewFolder();
                        break;
                    case Key.Y:
                        CreateNewAccount();
                        break;
                    case Key.E:
                        EditAccount();
                        break;
                    case Key.D:
                        DeleteAccount();
                        break;
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
            if ((_clearClipboardThread != null) && _clearClipboardThread.IsAlive)
            {
                _clearClipboardThread.Abort();
                Clipboard.SetText("");
            }

            //Checks if the user wants to save
            if (_needsSaving && DialogBox.QuestionDialogBox("Do you want to save before you quit?", true, this))
                Save();

            //Stops idle detection tread
            _idleDetectionThread.Abort();
        }

        #endregion

        #region Buttons

        #region Buttons - File

        private void NewSafeOnClick(object sender, RoutedEventArgs e)
        {
            NewSafe();
        }

        private void NewSafe()
        {
            //Gets new name
            string newName = DialogBox.TextInputDialogBox("Please enter the name for your new Safe:", "Create", "Cancel",
                this);

            if (string.IsNullOrEmpty(newName)) return;

            //Checks that safe name isn't already being used
            string[] files = Directory.GetFiles(@"Resources", "*.json");

            //Takes the name of each json file e.g. C:/Users/John/Documents/Safe/test.json => test
            files = files.Select(x => x.Split('\\').Last().Split('.')[0]).ToArray();

            if (files.Any(x => x == newName) &&
                !DialogBox.QuestionDialogBox(
                    "A file with that name already exists, are you sure you want to override it?", false, this))
                return;

            //Checks that that file name is valid
            if (newName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            {
                DialogBox.MessageDialogBox(
                    "A file's name cannot contain any of the following characters:\n\\/:*?\"<>|", this);
                return;
            }

            //Creates and opens new safe
            File.Create($"Resources\\{newName}.json").Close();
            MainWindow mainWindow = new MainWindow($"{newName}.json");
            try
            {
                mainWindow.Show();
            }
            catch (InvalidOperationException)
            {
                //The window mush have already closed itself for some reason and an error has already been displayed to the user
            }
            Close();
        }

        /// <summary>
        ///     Returns to login window
        /// </summary>
        private void OpenOnClick(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        /// <summary>
        ///     Saves the safe
        /// </summary>
        private void SaveOnClick(object sender, RoutedEventArgs e)
        {
            if (!_needsSaving) return;

            StartSaveThread();
        }

        /// <summary>
        ///     Asks for a new safe name and saves it when the button is clicked
        /// </summary>
        private void SaveAsOnClick(object sender, RoutedEventArgs e)
        {
            SaveAs();
        }

        /// <summary>
        ///     Asks for a new safe name and saves it
        /// </summary>
        private void SaveAs()
        {
            //Gets new name
            string newName = DialogBox.TextInputDialogBox("Please enter the name for your new Safe:", "Create", "Cancel",
                this);

            if (string.IsNullOrEmpty(newName)) return;

            //Checks that safe name isn't already being used
            string[] files = Directory.GetFiles(@"Resources", "*.json");

            //Takes the name of each json file e.g. C:/Users/John/Documents/Safe/test.json => test
            files = files.Select(x => x.Split('\\').Last().Split('.')[0]).ToArray();

            if (files.Any(x => x == newName) &&
                !DialogBox.QuestionDialogBox(
                    "A file with that name already exists, are you sure you want to override it?", false, this))
                return;

            //Checks that that file name is valid
            if (newName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            {
                DialogBox.MessageDialogBox(
                    "A file's name cannot contain any of the following characters:\n\\/:*?\"<>|", this);
                return;
            }

            File.Create($"Resources\\{newName}.json").Close();
            _openFile = $"{newName}.json";

            StartSaveThread();
        }

        /// <summary>
        ///     Locks the safe
        /// </summary>
        private void LockOnClick(object sender, RoutedEventArgs e)
        {
            LockSafe();
            _idleDetectionThread.Abort();
        }

        /// <summary>
        ///     Closes the window
        /// </summary>
        private void ExitOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Buttons - Edit

        /// <summary>
        ///     Creates a new folder when the button is clicked
        /// </summary>
        private void CreateNewFolderOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            CreateNewFolder();
        }

        /// <summary>
        ///     Creates a new instance of AccountEditorWindow to create a new account when the button is clicked
        /// </summary>
        private void CreateNewAccountOnClick(object sender, RoutedEventArgs e)
        {
            CreateNewAccount();
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

        #endregion

        #region Buttons - Folders

        /// <summary>
        ///     Gets new folder name from user and renames folder
        /// </summary>
        private void RenameFolderOnClick(object sender, RoutedEventArgs e)
        {
            //Gets the folder
            UIElement clickedObject = ((ContextMenu) ((MenuItem) sender).Parent).PlacementTarget;
            FolderExpander folder;
            if (clickedObject is Rectangle)
                folder = (FolderExpander) ((Rectangle) clickedObject).TemplatedParent;
            else
                folder = (FolderExpander) ((ContentPresenter) clickedObject).TemplatedParent;

            //Gets name to rename to
            string newFolderName = DialogBox.TextInputDialogBox("Enter new folder name", "Enter", "Cancel", this);
            if (string.IsNullOrWhiteSpace(newFolderName)) return;

            //Checks folder name is valid
            if (!VerifyFolderName(newFolderName))
            {
                DialogBox.MessageDialogBox(
                    "A folder's name cannot contain any of the following characters:\n\\/:*?\"<>|", this);
                return;
            }

            //Creates new path for the folder
            string newFolderPath = folder.Path;

            string[] splitPath = newFolderPath.Split('/');
            splitPath[splitPath.Length - 1] = newFolderName;
            newFolderPath = string.Join("/", splitPath);

            //Checks folder doesn't already exist
            if (FolderPathExists(newFolderPath))
            {
                DialogBox.MessageDialogBox("A folder with that name already exists", this);
                return;
            }

            //Gets folders index
            FolderExpander temp = folder;
            int index = ((StackPanel) temp.Parent).Children.IndexOf(temp);
            int numberOfDefaults = ((StackPanel) temp.Parent).Children.Cast<FolderExpander>().Count(x => x.Default);

            //Renames folder
            ChangeFolder(newFolderPath, index, numberOfDefaults, folder);
        }

        private void CreateNewFolderOnClickContextMenu(object sender, RoutedEventArgs e)
        {
            //Gets the folder
            UIElement clickedObject = ((ContextMenu) ((MenuItem) sender).Parent).PlacementTarget;
            FolderExpander folderToPutNewFolderIn;
            if (clickedObject is Rectangle)
                folderToPutNewFolderIn = (FolderExpander) ((Rectangle) clickedObject).TemplatedParent;
            else
                folderToPutNewFolderIn = (FolderExpander) ((ContentPresenter) clickedObject).TemplatedParent;

            //Creates new folder
            CreateNewFolder(folderToPutNewFolderIn.Path);
        }

        /// <summary>
        ///     Checks if a folder name is valid
        /// </summary>
        /// <param name="name">The folder name to check</param>
        /// <returns>True if folder name is valid</returns>
        private static bool VerifyFolderName(string name)
        {
            if (name.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
                return false;
            return true;
        }

        /// <summary>
        ///     Checks that a folder path exist
        /// </summary>
        /// <param name="path">The path to check</param>
        /// <returns>True if the path exists</returns>
        private static bool FolderPathExists(string path)
        {
            string[] pathParts = path.Split('/').Skip(1).ToArray();

            List<Folder> currentFolderList = SafeData.Folders;
            foreach (string pathPart in pathParts)
                if (currentFolderList.Exists(x => x.Name == pathPart))
                    currentFolderList = currentFolderList.Find(x => x.Name == pathPart).Children;
                else
                    return false;

            return true;
        }

        /// <summary>
        ///     Filters the Accounts to only those in the folder when double clicked
        /// </summary>
        private void FilterByFolderOnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2) return;

            FolderExpander parent;

            if (sender is ContentPresenter)
                parent = (FolderExpander) ((Grid) ((ContentPresenter) sender).Parent).TemplatedParent;
            else
                parent = (FolderExpander) ((Grid) ((Rectangle) sender).Parent).TemplatedParent;


            _folderFilter = parent.Path == "All" ? "" : parent.Path;

            FilterDataGrid();
        }

        #endregion

        #region Buttons - DataGrid

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
                char[] charArray = _profile.GetValue("Global", "VisibleColumns", "111111").ToCharArray();
                charArray[indexToChange] = '1';
                _profile.SetValue("Global", "VisibleColumns", new string(charArray));
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
                    char[] charArray = _profile.GetValue("Global", "VisibleColumns", "111111").ToCharArray();
                    charArray[indexToChange] = '0';
                    _profile.SetValue("Global", "VisibleColumns", new string(charArray));
                }
                else
                    menuItemClicked.IsChecked = true;
            }
        }

        #endregion

        #endregion

        #region Highlighting Folders

        /// <summary>
        ///     Highlights folder if it is hovered over
        /// </summary>
        private void HighlightFolderWhenMouseEnters(object sender, MouseEventArgs e)
        {
            ((FolderExpander) ((Grid) sender).TemplatedParent).IsHighlighted = true;
        }

        /// <summary>
        ///     Unhighlights folder when mouse if removed from it
        /// </summary>
        private void UnhighlightFolderWhenMouseLeaves(object sender, MouseEventArgs e)
        {
            ((FolderExpander) ((Grid) sender).TemplatedParent).IsHighlighted = false;
        }

        #endregion

        #region Folders

        /// <summary>
        ///     Create the folders in the folder menu
        /// </summary>
        /// <param name="folders">The list of folders to use to create the folder menu</param>
        private void ConstructFolders(IEnumerable<Folder> folders)
        {
            Folders.Children.RemoveRange(0, Folders.Children.Count);
            //Default folders
            Folders.Children.Add(new FolderExpander
            {
                Path = "All",
                Header = "All",
                Indentation = 10,
                Style = (Style) FindResource("DropDownFolder"),
                Default = true,
                HasSubFolders = false
            });

            foreach (Folder folder in folders) //Creates a folder for every folder in the SafeData
                if (folder.Children.Count == 0)
                    Folders.Children.Add(new FolderExpander
                    {
                        Path = $"/{folder.Name}",
                        Header = folder.Name,
                        Indentation = 10,
                        Style = (Style) FindResource("DropDownFolder"),
                        HasSubFolders = false
                    });
                else
                    Folders.Children.Add(MakeDropDownFolder(folder, $"/{folder.Name}"));
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
                Path = currentPath,
                Header = folder.Name,
                Indentation = (currentPath.Count(x => x == '/') - 1) * 10 + 10,
                Style = (Style) FindResource("DropDownFolder"),
                HasSubFolders = true
            };

            StackPanel stackPanel = new StackPanel();
            foreach (Folder childFolder in folder.Children)
                if (childFolder.Children.Count == 0)
                    stackPanel.Children.Add(new FolderExpander
                    {
                        Path = $"{currentPath}/{childFolder.Name}",
                        Header = childFolder.Name,
                        Indentation = ($"{currentPath}/{childFolder.Name}".Count(x => x == '/') - 1) * 10 + 10,
                        Style = (Style) FindResource("DropDownFolder"),
                        HasSubFolders = false
                    });
                else
                    stackPanel.Children.Add(MakeDropDownFolder(childFolder, $"{currentPath}/{childFolder.Name}"));
            output.Content = stackPanel;
            return output;
        }

        /// <summary>
        ///     Modifies the folder context menu when it is opened
        /// </summary>
        private void ModifyFolderLabelContextMenuOnOpening(object sender, ContextMenuEventArgs e)
        {
            //Gets the folder
            FolderExpander folder;
            ContextMenu contextMenu;
            if (sender is Rectangle)
            {
                contextMenu = ((Rectangle) sender).ContextMenu;
                folder = (FolderExpander) ((Rectangle) sender).TemplatedParent;
            }
            else
            {
                contextMenu = ((ContentPresenter) sender).ContextMenu;
                folder = (FolderExpander) ((ContentPresenter) sender).TemplatedParent;
            }

            //Hides rename option from default folders
            List<MenuItem> menuItems = contextMenu.Items.Cast<MenuItem>().ToList();
            MenuItem renameMenuItem = menuItems.Find(x => (string) x.Header == "Rename");
            renameMenuItem.Visibility = folder.Default ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        ///     Creates a new folder
        /// </summary>
        /// <param name="locationPath">Path of location to create the new folder, if blank it is places in the top level</param>
        private void CreateNewFolder(string locationPath = "")
        {
            StackPanel location;
            FolderExpander folderNewFolderGoesIn = null;

            //Gets name
            string folderName = DialogBox.TextInputDialogBox("Enter new folder name", "Enter", "Cancel", this);
            if (string.IsNullOrWhiteSpace(folderName)) return;

            //Checks folder name is valid
            if (!VerifyFolderName(folderName))
            {
                DialogBox.MessageDialogBox(
                    "A folder's name cannot contain any of the following characters:\n\\/:*?\"<>|", this);
                return;
            }

            //Checks folder doesn't already exist
            if (FolderPathExists($"{locationPath}/{folderName}"))
            {
                DialogBox.MessageDialogBox("A folder with that name already exists", this);
                return;
            }

            //Puts folder in the SafeData
            List<Folder> currentFolderList = SafeData.Folders;
            List<string> pathParts = locationPath.Split('/').Skip(1).ToList();
            currentFolderList = pathParts.Aggregate(currentFolderList,
                (current, part) => current.Find(x => x.Name == part).Children);

            currentFolderList.Add(new Folder {Name = folderName, Children = new List<Folder>()});

            //Gets stackpanel that new folder goes in
            if (locationPath == "")
                location = Folders;
            else
            {
                folderNewFolderGoesIn = GetFolderFromPath(locationPath);
                location = (StackPanel) folderNewFolderGoesIn.Content;
            }

            //Sets up parent folder if it currently has no children
            if (location == null)
            {
                location = new StackPanel();
                folderNewFolderGoesIn.Content = location;
                folderNewFolderGoesIn.HasSubFolders = true;
            }

            //Creates new folder
            location.Children.Add(new FolderExpander
            {
                Path = $"{locationPath}/{folderName}",
                Header = folderName,
                Indentation = ($"{locationPath}/{folderName}".Count(x => x == '/') - 1) * 10 + 10,
                Style = (Style) FindResource("DropDownFolder"),
                HasSubFolders = false
            });
        }

        /// <summary>
        ///     Change the location and/or name of a folder
        /// </summary>
        /// <param name="newPath">New path for the folder</param>
        /// <param name="newPathIndex">The index for the folder in its new location</param>
        /// <param name="numberOfDefaults">Number of default folders at the start of the level the folder is in</param>
        /// <param name="folder">The FolderLabel or FolderExpander being moved</param>
        private void ChangeFolder(string newPath, int newPathIndex, int numberOfDefaults, FolderExpander folder)
        {
            _needsSaving = true;

            string oldPath = folder.Path;

            string[] oldPathParts = oldPath.Split('/').Skip(1).ToArray();
            string[] newPathParts = newPath.Split('/').Skip(1).ToArray();

            /*First changes everything in the main data structure*/
            //Finds and deletes the folder using the path
            List<Folder> currentFolderList = SafeData.Folders;
            for (int i = 0; i < oldPathParts.Length - 1; i++)
                currentFolderList = currentFolderList.Find(x => x.Name == oldPathParts[i]).Children;

            Folder folderToModify = currentFolderList.Find(x => x.Name == oldPathParts.Last());

            currentFolderList.Remove(folderToModify);

            //Recreates the folder at the new path
            currentFolderList = SafeData.Folders;
            for (int i = 0; i < newPathParts.Length - 1; i++)
                currentFolderList = currentFolderList.Find(x => x.Name == newPathParts[i]).Children;

            currentFolderList.Insert(newPathIndex - numberOfDefaults, folderToModify);

            folderToModify.Name = newPathParts.Last();

            //Changes account path data
            foreach (Account account in _accountsObservableCollection)
                if (account.Path.StartsWith(oldPath))
                    account.Path = newPath + account.Path.Substring(oldPath.Length);

            /*Then changes the already existing folder controls*/
            //Gets the folder
            FolderExpander folderBeingMoved = GetFolderFromPath(oldPath);

            FolderExpander folderBeingMovedParentFolder =
                ((StackPanel) folderBeingMoved.Parent).Parent as FolderExpander;

            //Removes the folder from its current location
            ((StackPanel) folderBeingMoved.Parent).Children.Remove(folderBeingMoved);

            //If the parent folder is now empty it removes the dropdown
            if ((folderBeingMovedParentFolder != null) &&
                (((StackPanel) folderBeingMovedParentFolder.Content).Children.Count == 0))
            {
                folderBeingMovedParentFolder.Content = null;
                folderBeingMovedParentFolder.HasSubFolders = false;
                folderBeingMovedParentFolder.IsExpanded = false;
            }

            //Gets where the folding is going to be moved to
            FolderExpander folderMovedInto =
                GetFolderFromPath("/" + string.Join("/", newPathParts.Take(newPathParts.Length - 1)));

            //Modifies the folder being dropped into if it had no subfolders before hand
            if (folderMovedInto != null)
                if (folderMovedInto.Content == null)
                {
                    folderMovedInto.Content = new StackPanel();
                    folderMovedInto.HasSubFolders = true;
                }

            //Changes folder values and places it in its new location
            folderBeingMoved.Header = newPathParts.Last();
            folderBeingMoved.Path = newPath;
            folderBeingMoved.Indentation = (newPath.Count(x => x == '/') - 1) * 10 + 10;
            if (folderMovedInto != null)
                ((StackPanel) folderMovedInto.Content).Children.Insert(newPathIndex, folderBeingMoved);
            else
                Folders.Children.Insert(newPathIndex, folderBeingMoved);

            //Changes the path of all the sub folders
            ChangeSubFolderPathsAndIndentations(folder);
        }

        /// <summary>
        ///     Gets folder from path
        /// </summary>
        /// <param name="path">Path the folder is located at</param>
        /// <returns>The FolderExpander the path points to</returns>
        private FolderExpander GetFolderFromPath(string path)
        {
            string[] pathParts = path.Split('/').Skip(1).ToArray();
            FolderExpander folder =
                Folders.Children.Cast<FolderExpander>().ToList().Find(x => (string) x.Header == pathParts[0]);
            for (int i = 1; i < pathParts.Length; i++)
                folder =
                    ((StackPanel) folder.Content).Children.Cast<FolderExpander>()
                        .ToList()
                        .Find(x => (string) x.Header == pathParts[i]);
            return folder;
        }

        /// <summary>
        ///     Changes the start of all child folder's paths to match the parent folder's path
        /// </summary>
        /// <param name="folder">The folder to change the children for</param>
        private void ChangeSubFolderPathsAndIndentations(FolderExpander folder)
        {
            if (!folder.HasSubFolders) return;
            List<FolderExpander> childFolders = ((StackPanel) folder.Content).Children.Cast<FolderExpander>().ToList();
            string newStartOfPath = folder.Path;
            string[] temp = childFolders[0].Path.Split('/');
            string oldStartOfPath = string.Join("/", temp.Take(temp.Length - 1));

            foreach (FolderExpander childFolder in childFolders)
            {
                childFolder.Path = newStartOfPath + childFolder.Path.Substring(oldStartOfPath.Length);
                childFolder.Indentation = (childFolder.Path.Count(x => x == '/') - 1) * 10 + 10;
                ChangeSubFolderPathsAndIndentations(childFolder);
            }
        }

        #endregion

        #region Account Modification

        /// <summary>
        ///     Creates a new instance of AccountEditorWindow to create a new account
        /// </summary>
        private void CreateNewAccount()
        {
            if (Application.Current.Windows.OfType<MetroWindow>().Any(x => x.Title == "AccountEditorWindow"))
                return; //Check if a account editor window is already open

            Account newAccount = new Account();
            AccountEditorWindow accountEditorWindow = new AccountEditorWindow(true, newAccount) {Owner = this};
            if (accountEditorWindow.ShowDialog() != true) return;
            _needsSaving = true;
            _accountsObservableCollection.Add(accountEditorWindow.AccountBeingEdited);
        }

        /// <summary>
        ///     Creates a new instance of AccountEditorWindow to edit the currently selected account
        /// </summary>
        private void EditAccount()
        {
            if (Application.Current.Windows.OfType<MetroWindow>().Any(x => x.Title == "AccountEditorWindow") ||
                (AccountList.SelectedItem == null))
                return; //Check if a editor window window is already open or no account is selected

            Account editedAccount = (Account) AccountList.SelectedItem;
            AccountEditorWindow accountEditorWindow = new AccountEditorWindow(false, editedAccount) {Owner = this};
            if (accountEditorWindow.ShowDialog() != true) return;
            _needsSaving = true;
            _accountsObservableCollection[
                    _accountsObservableCollection.ToList()
                        .FindIndex(x => x.Id == accountEditorWindow.AccountBeingEdited.Id)
                ] =
                accountEditorWindow.AccountBeingEdited;
            AccountList.Items.Refresh();
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
                Account[] accountsToDelete = AccountList.SelectedItems.Cast<Account>().ToArray();
                foreach (Account account in accountsToDelete)
                    _accountsObservableCollection.Remove(account);
                _needsSaving = true;
            }
        }

        #endregion

        #region DataGrid

        /// <summary>
        ///     Fills AccountList DataGrid with all the accounts
        /// </summary>
        private void ConstructAccountEntries()
        {
            _accountsObservableCollection = new ObservableCollection<Account>();
            SafeData.Accounts.Where(
                    x =>
                        string.IsNullOrEmpty(_folderFilter) ||
                        (!string.IsNullOrEmpty(x.Path) && x.Path.StartsWith(_folderFilter)))
                .ToList()
                .ForEach(x => _accountsObservableCollection.Add(x));

            _accountsCollectionViewSource = new CollectionViewSource {Source = _accountsObservableCollection};
            _accountsICollectionView = _accountsCollectionViewSource.View;

            AccountList.ItemsSource = _accountsICollectionView;
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

            bool isHidden = _profile.GetValue("Global", "VisibleColumns", "111111")[index] == '0';
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
                if (((TextBlock) sender).Name == "URL")
                    Process.Start(((TextBlock) sender).Text);
                else
                    CopyCell((TextBlock) sender);
        }

        /// <summary>
        ///     Copies the content of the cell to the clipboard
        /// </summary>
        /// <param name="cell">The cell to copy the data from</param>
        private void CopyCell(TextBlock cell)
        {
            if (cell is PasswordTextBlock)
                Clipboard.SetText(((PasswordTextBlock) cell).Password);
            else
                Clipboard.SetText(cell.Text);

            //Terminates any currently running ClearClipboard threads
            if ((_clearClipboardThread != null) && _clearClipboardThread.IsAlive)
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
                Dispatcher.Invoke(
                    () =>
                        MessageBox.Content =
                            $"Field copied to clipboard. {secondsLeft} seconds till clipboard is cleared");
                Thread.Sleep(1000);
            }
            Dispatcher.Invoke(() => Clipboard.SetText(""));
            //This line of code can throw a System.Runtime.InteropServices.COMException if the user happens to be pasting at the time of this code being run
            Dispatcher.Invoke(() => MessageBox.Content = "");
        }

        /// <summary>
        ///     Filters the DataGrid based on what is in the search box and what folder has been selected
        /// </summary>
        private void FilterDataGrid()
        {
            DateTime start = DateTime.Now;
            _filter = x =>
            {
                Account account = (Account) x;
                return account.Path.StartsWith(_folderFilter) &&
                       (account.AccountName.ToUpper().Contains(SearchBox.Text.ToUpper()) ||
                        account.Notes.ToUpper().Contains(SearchBox.Text.ToUpper()));
            };

            _accountsICollectionView.Filter = _filter;
            Debug.WriteLine(DateTime.Now - start);
        }

        #endregion

        #region Saving

        /// <summary>
        ///     Starts a new thread to save the safe
        /// </summary>
        private void StartSaveThread()
        {
            _saveThread?.Abort();

            _saveThread = new Thread(Save);
            _saveThread.Start();
        }

        /// <summary>
        ///     Saves the safe
        /// </summary>
        private void Save()
        {
            _needsSaving = false;
            SafeData.Accounts = new List<Account>(_accountsObservableCollection);
            string jsonText = JsonConvert.SerializeObject(SafeData);
            File.WriteAllText($"Resources\\{_openFile}.bak", jsonText);
            File.WriteAllText($"Resources\\{_openFile}", jsonText);
            File.Delete($"Resources\\{_openFile}.bak");
            Dispatcher.Invoke(() => MessageBox.Content = "Safe Saved");
        }

        #endregion

        #region Drag and Drop

        #region Start Drag Events

        /// <summary>
        ///     Starts a drag event of the selected account when you hold down your mouse and move it
        /// </summary>
        private void StartDraggingofAccount(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            object selectedItem = AccountList.SelectedItem;
            if (selectedItem == null) return;

            DataGridRow container =
                (DataGridRow) AccountList.ItemContainerGenerator.ContainerFromItem(selectedItem);
            if (container != null)
                DragDrop.DoDragDrop(container, selectedItem, DragDropEffects.Move);
        }

        private void StartDraggingOfFolder(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (((FolderExpander) sender).Default) return;

            DependencyObject container = ((FolderExpander) sender).Parent;
            DragDrop.DoDragDrop(container, sender, DragDropEffects.Move);
        }

        #endregion

        #region DragOver and DragLeave Events

        /// <summary>
        ///     Highlights a folder when you hover over it with something to drop
        /// </summary>
        private void DragOverFolder(object sender, DragEventArgs e)
        {
            FolderExpander folder = (FolderExpander) ((Grid) sender).TemplatedParent;

            //Highlights folder if it is a account being dragged onto it
            if (e.Data.GetData(typeof(Account)) is Account)
            {
                if (!folder.IsHighlighted)
                    folder.IsHighlighted = true;
            }

            //Highlights and adds borders if it is a folder being dragged onto it
            else if (e.Data.GetData(typeof(FolderExpander)) is FolderExpander)
            {
                FolderExpander dropTarget = (FolderExpander) ((Grid) sender).TemplatedParent;

                Point mousePosition = e.GetPosition(dropTarget);

                //Folder is hovered above
                if (mousePosition.Y <= 5)
                {
                    if (folder.IsHighlighted)
                        folder.IsHighlighted = false;

                    if (!folder.DisplayTopBlackBar)
                        folder.DisplayTopBlackBar = true;

                    if (folder.DisplayBottomBlackBar)
                        folder.DisplayBottomBlackBar = false;
                }
                //Folder was hovered below
                else if (mousePosition.Y >= dropTarget.ActualHeight - 5)
                {
                    if (folder.IsHighlighted)
                        folder.IsHighlighted = false;

                    if (folder.DisplayTopBlackBar)
                        folder.DisplayTopBlackBar = false;

                    if (!folder.DisplayBottomBlackBar)
                        folder.DisplayBottomBlackBar = true;
                }
                //Folder was hovered inside
                else
                {
                    if (!folder.IsHighlighted)
                        folder.IsHighlighted = true;

                    if (folder.DisplayTopBlackBar)
                        folder.DisplayTopBlackBar = false;

                    if (folder.DisplayBottomBlackBar)
                        folder.DisplayBottomBlackBar = false;
                }
            }
        }

        private void DragLeaveFolder(object sender, DragEventArgs e)
        {
            FolderExpander folder = (FolderExpander) ((Grid) sender).TemplatedParent;

            //Unhighlights folder if it is a account being dragged off it
            if (e.Data.GetData(typeof(Account)) is Account)
            {
                if (folder.IsHighlighted)
                    folder.IsHighlighted = false;
            }

            //Unhighlights and removes borders if it is a folder being dragged off it
            else if (e.Data.GetData(typeof(FolderExpander)) is FolderExpander)
            {
                if (folder.IsHighlighted)
                    folder.IsHighlighted = false;

                if (folder.DisplayTopBlackBar)
                    folder.DisplayTopBlackBar = false;

                if (folder.DisplayBottomBlackBar)
                    folder.DisplayBottomBlackBar = false;
            }
        }

        #endregion

        #region Drop Events

        /// <summary>
        ///     Changes an account or folder's path an account or folder is dropped on it
        /// </summary>
        private void DropOntoFolder(object sender, DragEventArgs e)
        {
            FolderExpander dropTarget = (FolderExpander) ((Grid) sender).TemplatedParent;
            if (dropTarget.IsHighlighted)
                dropTarget.IsHighlighted = false;

            if (dropTarget.DisplayTopBlackBar)
                dropTarget.DisplayTopBlackBar = false;

            if (dropTarget.DisplayBottomBlackBar)
                dropTarget.DisplayBottomBlackBar = false;

            Thread reEnableBackgroundDropsThread;

            if (e.Data.GetData(typeof(Account)) is Account)
            {
                Account account = (Account) e.Data.GetData(typeof(Account));

                string newPath = dropTarget.Path;

                if (newPath == "All")
                    _accountsObservableCollection[_accountsObservableCollection.IndexOf(account)].Path = "";
                else
                    _accountsObservableCollection[_accountsObservableCollection.IndexOf(account)].Path = newPath;
                FilterDataGrid();
            }

            else if (e.Data.GetData(typeof(FolderExpander)) is FolderExpander)
            {
                Point mousePosition = e.GetPosition(dropTarget);

                //Folder was dropped above
                if (mousePosition.Y <= 5)
                {
                    MoveFolderAboveOrBelow(e, dropTarget, true);
                }
                //Folder was dropped below
                else if (mousePosition.Y >= dropTarget.ActualHeight - 5)
                {
                    MoveFolderAboveOrBelow(e, dropTarget, false);
                }
                //Folder was dropped inside
                else
                {
                    if (dropTarget.Default) return;

                    FolderExpander folder = (FolderExpander) e.Data.GetData(typeof(FolderExpander));

                    if (folder == null) return;
                    //Checks you arn't putting the folder in itself
                    if (dropTarget.Path.StartsWith(folder.Path))
                    {
                        //Prevents folder from being moved as if it was dropped on blank space
                        _preventFolderDropOntoFolderArea = true;
                        reEnableBackgroundDropsThread = new Thread(ReEnableBackgroundDrops);
                        reEnableBackgroundDropsThread.Start();
                        return;
                    }

                    //Checks if you are putting a folder in the folder it is already in
                    if (dropTarget.Path ==
                        string.Join("/", folder.Path.Split('/').Take(folder.Path.Split('/').Length - 1)))
                        return;

                    //Generates new path
                    string newPath = $"{dropTarget.Path}/{folder.Header}";

                    //Gets the new folder index
                    int newIndex = 0;
                    if ((StackPanel) dropTarget.Content != null)
                        newIndex = ((StackPanel) dropTarget.Content).Children.Count;

                    //Changes the folder path
                    ChangeFolder(newPath, newIndex, 0, folder);
                }
            }

            //Prevents folder from being moved as if it was dropped on blank space
            _preventFolderDropOntoFolderArea = true;
            reEnableBackgroundDropsThread = new Thread(ReEnableBackgroundDrops);
            reEnableBackgroundDropsThread.Start();
        }

        /// <summary>
        ///     Moves folder above or below another
        /// </summary>
        /// <param name="e">Drag event</param>
        /// <param name="dropTarget">The folder it is being dropped on</param>
        /// <param name="wasDroppedAbove">True if the folder was dropped above</param>
        private void MoveFolderAboveOrBelow(DragEventArgs e, FolderExpander dropTarget,
            bool wasDroppedAbove)
        {
            FolderExpander folderBeingMoved = (FolderExpander) e.Data.GetData(typeof(FolderExpander));

            if (folderBeingMoved == null) return;
            //Checks you arn't putting the folder in itself
            if (dropTarget.Path.StartsWith(folderBeingMoved.Path)) return;

            //Generates new path
            StackPanel movingInto = (StackPanel) dropTarget.Parent;
            string newPath;
            if (movingInto.Parent is FolderExpander)
                newPath = $"{((FolderExpander) movingInto.Parent).Path}/{folderBeingMoved.Header}";
            else
                newPath = $"/{folderBeingMoved.Header}";

            //Calculates if one should be subtracted based on if the removal of that folder affects the index
            bool subrtactOne = movingInto.Children.Contains(folderBeingMoved) &&
                               (movingInto.Children.IndexOf(folderBeingMoved) <
                                movingInto.Children.IndexOf(dropTarget));
            //Gets the new folder index
            int newIndex = movingInto.Children.IndexOf(dropTarget) + (wasDroppedAbove ? 0 : 1) + (subrtactOne ? -1 : 0);
            //Gets the number of default folders
            int numberOfDefaults = movingInto.Children.Cast<FolderExpander>().Count(x => x.Default);

            //Changes the folder path
            ChangeFolder(newPath, newIndex, numberOfDefaults, folderBeingMoved);
        }

        /// <summary>
        ///     Changes a folder's path when a folder is dropped on it
        /// </summary>
        private void DropOntoFolderArea(object sender, DragEventArgs e)
        {
            StackPanel dropTarget = (StackPanel) sender;

            if (e.Data.GetData(typeof(FolderExpander)) is FolderExpander)
            {
                if (_preventFolderDropOntoFolderArea) return;

                FolderExpander folder = (FolderExpander) e.Data.GetData(typeof(FolderExpander));

                if (folder == null) return;

                //Generates new path
                string newPath = $"/{folder.Header}";

                //Gets the new folder index and number of defaults
                int newIndex = dropTarget.Children.Count;
                if (dropTarget.Children.Contains(folder))
                    newIndex--;
                int numberOfDefaults = dropTarget.Children.OfType<FolderExpander>().Count(x => x.Default);

                //Changes the folder path
                ChangeFolder(newPath, newIndex, numberOfDefaults, folder);
            }
        }

        /// <summary>
        ///     Re-enables the the ability to drop folder onto the folder area after 100ms
        /// </summary>
        private void ReEnableBackgroundDrops()
        {
            Thread.Sleep(100);
            _preventFolderDropOntoFolderArea = false;
        }

        #endregion

        #endregion
    }
}