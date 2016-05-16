using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;

namespace MockupApplication
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Height = (SystemParameters.PrimaryScreenHeight * 0.75);
            Width = (SystemParameters.PrimaryScreenWidth * 0.75);
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

        #region Section Resizing

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid g = (Grid) sender;

            double maxW = e.NewSize.Width - g.ColumnDefinitions[2].MinWidth - g.ColumnDefinitions[1].ActualWidth;
            g.ColumnDefinitions[0].MaxWidth = maxW;
        }

        #endregion

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

        #region Top Control Buttons

        /// <summary>
        ///     Close button is clicked
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        /// <summary>
        ///     Maximised button is clicked
        /// </summary>
        private void MaximisedButton_Clicked(object sender, RoutedEventArgs e)
        {
            AdjustWindowSize();
        }

        /// <summary>
        ///     Minimised button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinimisedButton_Clicked(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        #endregion

        #region ResizeWindows

        private bool _resizeInProcess;

        private void Resize_Init(object sender, MouseButtonEventArgs e)
        {
            Rectangle senderRect = sender as Rectangle;
            _resizeInProcess = true;
            senderRect.CaptureMouse();
        }

        private void Resize_End(object sender, MouseButtonEventArgs e)
        {
            Rectangle senderRect = sender as Rectangle;
            _resizeInProcess = false;
            senderRect.ReleaseMouseCapture();
        }

        private void Resizeing_Form(object sender, MouseEventArgs e)
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
            ChangeResizeRectangles();
        }

        private void ChangeResizeRectangles()
        {
            if (WindowState == WindowState.Maximized)
            {
                Panel.SetZIndex(TopSizeGrip, -1);
                Panel.SetZIndex(BottomSizeGrip, -1);
                Panel.SetZIndex(LeftSizeGrip, -1);
                Panel.SetZIndex(RightSizeGrip, -1);
                Panel.SetZIndex(TopLeftSizeGrip, -1);
                Panel.SetZIndex(TopRightSizeGrip, -1);
                Panel.SetZIndex(BottomLeftSizeGrip, -1);
                Panel.SetZIndex(BottomRightSizeGrip, -1);
            }
            else
            {
                Panel.SetZIndex(TopSizeGrip, 1);
                Panel.SetZIndex(BottomSizeGrip, 1);
                Panel.SetZIndex(LeftSizeGrip, 1);
                Panel.SetZIndex(RightSizeGrip, 1);
                Panel.SetZIndex(TopLeftSizeGrip, 1);
                Panel.SetZIndex(TopRightSizeGrip, 1);
                Panel.SetZIndex(BottomLeftSizeGrip, 1);
                Panel.SetZIndex(BottomRightSizeGrip, 1);
            }
        }

        #endregion

        #region Dragging window

        /// <summary>
        ///     Alowes window to be dragged or resized
        /// </summary>
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                AdjustWindowSize();
            else
                DragMove();
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
        }

        /// <summary>
        ///     Stops window from being dragged
        /// </summary>
        private void TitleBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        #endregion
    }
}