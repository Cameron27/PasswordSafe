using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            const string json = @"{
	                            ""filesystem"":
	                            {
		                            ""Common"":{},
		                            ""Folders"":
		                            {
			                            ""Personal"":{},
			                            ""WorkWorkWorkWorkWork"":{},
			                            ""School"":
			                            {
				                            ""High School"":{},
				                            ""Uni"":{}
			                            }
		                            }
	                            }
                            }";
            JObject o = JObject.Parse(json);
            ConstructFolders((JObject) o["filesystem"]);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        #region Construct Folders

        private void ConstructFolders(JObject o)
        {
            Menu.Children.Add(new Label
            {
                Content = "All",
                Padding = new Thickness(10, 5, 5, 5),
                Style = (Style) FindResource("Folder")
            });
            Menu.Children.Add(new Label
            {
                Content = "Prediction",
                Padding = new Thickness(10, 5, 5, 5),
                Style = (Style) FindResource("Folder")
            });
            foreach (KeyValuePair<string, JToken> pair in o)
            {
                if (!pair.Value.Any())
                    Menu.Children.Add(new Label
                    {
                        Content = pair.Key,
                        Padding = new Thickness(10, 5, 5, 5),
                        Style = (Style) FindResource("Folder")
                    });
                else
                    Menu.Children.Add(MakeDropDownFolder(pair, 1));
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

        #region Resize Window

        /// <summary>
        ///     Adjusts grid settings for min and max sizes when overall window size changes
        /// </summary>
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid g = (Grid) sender;

            double maxW = e.NewSize.Width - g.ColumnDefinitions[2].MinWidth - g.ColumnDefinitions[1].ActualWidth;
            g.ColumnDefinitions[0].MaxWidth = maxW;
        }

        private bool _resizeInProcess;

        private void ResizeRectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle senderRect = sender as Rectangle;
            _resizeInProcess = true;
            senderRect.CaptureMouse();
        }

        private void ResizeRectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Rectangle senderRect = sender as Rectangle;
            _resizeInProcess = false;
            senderRect.ReleaseMouseCapture();
        }

        private void ResizeRectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (_resizeInProcess)
            {
                Rectangle senderRect = sender as Rectangle;
                Window mainWindow = senderRect.Tag as Window;
                double width = e.GetPosition(mainWindow).X;
                double height = e.GetPosition(mainWindow).Y;
                senderRect.CaptureMouse();
                if (senderRect.Name.ToLower().Contains("right"))
                {
                    width += 5;
                    if (width > 0)
                        mainWindow.Width = width;
                }
                if (senderRect.Name.ToLower().Contains("left"))
                {
                    width -= 5;
                    mainWindow.Left += width;
                    width = mainWindow.Width - width;
                    if (width > 0)
                        mainWindow.Width = width;
                }
                if (senderRect.Name.ToLower().Contains("bottom"))
                {
                    height += 5;
                    if (height > 0)
                        mainWindow.Height = height;
                }
                if (senderRect.Name.ToLower().Contains("top"))
                {
                    height -= 5;
                    mainWindow.Top += height;
                    height = mainWindow.Height - height;
                    if (height > 0)
                        mainWindow.Height = height;
                }
            }
        }

        /// <summary>
        ///     Adjusts the WindowSize to correct parameters when Maximize button is clicked
        /// </summary>
        private void AdjustWindowSize()
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        #endregion

        #region Dragging window

        private bool _mRestoreIfMove;

        private void rctHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                AdjustWindowSize();

                return;
            }

            if (WindowState == WindowState.Maximized)
            {
                _mRestoreIfMove = true;
                return;
            }

            DragMove();

            if (WindowState == WindowState.Normal && Math.Abs(GetMousePosition().Y) <= 0)
            {
                AdjustWindowSize();
            }
        }


        private void rctHeader_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _mRestoreIfMove = false;
        }


        private void rctHeader_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mRestoreIfMove)
            {
                _mRestoreIfMove = false;

                double percentHorizontal = e.GetPosition(this).X / ActualWidth;
                double targetHorizontal = RestoreBounds.Width * percentHorizontal;

                double percentVertical = e.GetPosition(this).Y / ActualHeight;
                double targetVertical = RestoreBounds.Height * percentVertical;

                WindowState = WindowState.Normal;

                Win32Point lMousePosition;
                GetCursorPos(out lMousePosition);

                Left = lMousePosition.X - targetHorizontal;
                Top = lMousePosition.Y - targetVertical;

                DragMove();
            }
        }


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out Win32Point lpPoint);


        [StructLayout(LayoutKind.Sequential)]
        public struct Win32Point
        {
            public int X;
            public int Y;

            public Win32Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        public static Point GetMousePosition()
        {
            Win32Point w32Mouse;
            GetCursorPos(out w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        #endregion
    }
}