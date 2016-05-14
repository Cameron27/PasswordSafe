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
            ConstructMenu((JObject) o["filesystem"]);
        }

        private void ConstructMenu(JObject o)
        {
            Menu.Children.Add(new Label
            {
                Content = "All",
                Padding = new Thickness(5, 5, 5, 5),
                Style = (Style) FindResource("MenuItem")
            });
            Menu.Children.Add(new Label
            {
                Content = "Prediction",
                Padding = new Thickness(5, 5, 5, 5),
                Style = (Style) FindResource("MenuItem")
            });
            foreach (KeyValuePair<string, JToken> pair in o)
            {
                if (!pair.Value.Any())
                    Menu.Children.Add(new Label {Content = pair.Key, Style = (Style) FindResource("MenuItem")});
                else
                    Menu.Children.Add(MakeDropDownMenu(pair, 1));
            }
        }

        private Expander MakeDropDownMenu(KeyValuePair<string, JToken> pair, int depth)
        {
            Expander output = new Expander
            {
                Header = pair.Key,
                Padding = new Thickness((depth - 1) * 10, 0, 0, 0),
                Template = (ControlTemplate) FindResource("DropDownMenu")
            };
            StackPanel stackPanel = new StackPanel();
            foreach (KeyValuePair<string, JToken> pair2 in (JObject) pair.Value)
            {
                if (!pair2.Value.Any())
                    stackPanel.Children.Add(new Label
                    {
                        Content = pair2.Key,
                        Padding = new Thickness(depth * 10 + 5, 5, 5, 5),
                        Style = (Style) FindResource("MenuItem")
                    });
                else
                    stackPanel.Children.Add(MakeDropDownMenu(pair2, depth + 1));
            }
            output.Content = stackPanel;
            return output;
        }

        /// <summary>
        ///     Alowes window to be draged or resized
        /// </summary>
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                if (e.ClickCount == 2)
                    AdjustWindowSize();
                else
                    Application.Current.MainWindow.DragMove();
        }

        /// <summary>
        ///     Close button is clicked
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid g = (Grid) sender;

            double maxW = e.NewSize.Width - g.ColumnDefinitions[2].MinWidth - g.ColumnDefinitions[1].ActualWidth;
            g.ColumnDefinitions[0].MaxWidth = maxW;
        }

        /// <summary>
        ///     Maximised button is clicked
        /// </summary>
        private void MaximisedButton_Clicked(object sender, RoutedEventArgs e)
        {
            AdjustWindowSize();
        }

        /// <summary>
        ///     Adjusts the WindowSize to correct parameters when Maximize button is clicked
        /// </summary>
        private void AdjustWindowSize()
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
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

        #endregion
    }
}