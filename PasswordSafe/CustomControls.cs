using System.Windows;
using System.Windows.Controls;

namespace PasswordSafe
{
    namespace CustomControls
    {
        public class FolderLabel : Label
        {
            public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Path",
                typeof(string),
                typeof(string));

            public string Path
            {
                get { return GetValue(SourceProperty) as string; }
                set { SetValue(SourceProperty, value); }
            }
        }

        public class FolderExpander : Expander
        {
            public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("PathPart",
                typeof(string),
                typeof(string));

            public string PathPart
            {
                get { return GetValue(SourceProperty) as string; }
                set { SetValue(SourceProperty, value); }
            }
        }
    }
}