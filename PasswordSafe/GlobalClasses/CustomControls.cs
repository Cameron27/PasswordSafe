using System.Windows;
using System.Windows.Controls;

namespace PasswordSafe.GlobalClasses
{
    namespace CustomControls
    {
        public class FolderLabel : Label
        {
            public static readonly DependencyProperty PathProperty = DependencyProperty.Register("Path",
                typeof(string),
                typeof(string));

            public string Path
            {
                get { return (string) GetValue(PathProperty); }
                set { SetValue(PathProperty, value); }
            }
        }

        public class FolderExpander : Expander
        {
            public static readonly DependencyProperty PathPartProperty = DependencyProperty.Register("PathPart",
                typeof(string),
                typeof(string));

            public string PathPart
            {
                get { return (string) GetValue(PathPartProperty); }
                set { SetValue(PathPartProperty, value); }
            }
        }

        public class PasswordTextBlock : TextBlock
        {
            public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register("Password",
                typeof(string),
                typeof(string));

            public string Password
            {
                get { return (string) GetValue(PasswordProperty); }
                set { SetValue(PasswordProperty, value); }
            }
        }

        public class FolderComboBoxItem : ComboBoxItem
        {
            public static readonly DependencyProperty IndentationProperty = DependencyProperty.Register("Indentation",
                typeof(double),
                typeof(double));

            public static readonly DependencyProperty EndPathProperty = DependencyProperty.Register("EndPath",
                typeof(Visibility),
                typeof(Visibility));

            public double Indentation
            {
                get { return (double) GetValue(IndentationProperty); }
                set { SetValue(IndentationProperty, value); }
            }

            public Visibility EndPath
            {
                get { return (Visibility) GetValue(EndPathProperty); }
                set { SetValue(EndPathProperty, value); }
            }
        }
    }
}