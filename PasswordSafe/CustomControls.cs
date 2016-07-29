using System.Windows;
using System.Windows.Controls;

namespace PasswordSafe
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
                get { return GetValue(PathProperty) as string; }
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
                get { return GetValue(PathPartProperty) as string; }
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
                get { return GetValue(PasswordProperty) as string; }
                set { SetValue(PasswordProperty, value); }
            }
        }
    }
}