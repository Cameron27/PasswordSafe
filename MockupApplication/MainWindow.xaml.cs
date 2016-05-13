using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
		                            ""CommonCommonCommon"":{},
		                            ""Folders"":
		                            {
			                            ""Personal"":{},
			                            ""Work"":{},
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
            Menu.Children[0].SetValue(Grid.RowProperty, 0);
            Menu.Children[1].SetValue(Grid.RowProperty, 1);
            int gridCount = 2;
            foreach (KeyValuePair<string, JToken> pair in o)
            {
                Menu.RowDefinitions.Add(new RowDefinition());
                Menu.RowDefinitions[gridCount].Height = GridLength.Auto;
                if (!pair.Value.Any())
                {
                    Control tempControl = new Label {Content = pair.Key, Style = (Style) FindResource("MenuItem")};
                    Menu.Children.Add(tempControl);
                    tempControl.SetValue(Grid.RowProperty, gridCount);
                }
                else
                {
                    Control tempControl = MakeDropDownMenu(pair, 1);
                    Menu.Children.Add(tempControl);
                    tempControl.SetValue(Grid.RowProperty, gridCount);
                }
                gridCount++;
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

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }
    }
}