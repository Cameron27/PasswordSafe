using System.Windows;
using System.Windows.Controls;

namespace PasswordSafe.GlobalClasses
{
    namespace CustomControls
    {
        public class FolderExpander : Expander
        {
            public static readonly DependencyProperty PathProperty = DependencyProperty.Register("Path",
                typeof(string), typeof(string));

            public static readonly DependencyProperty SubFoldersProperty = DependencyProperty.Register("SubFolders",
                typeof(bool), typeof(FolderExpander));

            public static readonly DependencyProperty DefaultProperty = DependencyProperty.Register("Default",
                typeof(bool), typeof(FolderExpander));

            public static readonly DependencyProperty IndentationProperty = DependencyProperty.Register("Indentation",
                typeof(double), typeof(FolderExpander));

            public string Path
            {
                get { return (string) GetValue(PathProperty); }
                set { SetValue(PathProperty, value); }
            }

            public bool SubFolders
            {
                get { return (bool) GetValue(SubFoldersProperty); }
                set { SetValue(SubFoldersProperty, value); }
            }

            public bool Default
            {
                get { return (bool) GetValue(DefaultProperty); }
                set { SetValue(DefaultProperty, value); }
            }

            public double Indentation
            {
                get { return (double) GetValue(IndentationProperty); }
                set { SetValue(IndentationProperty, value); }
            }
        }

        public class PasswordTextBlock : TextBlock
        {
            public static readonly DependencyProperty PasswordProperty = DependencyProperty.Register("Password",
                typeof(string), typeof(string));

            public string Password
            {
                get { return (string) GetValue(PasswordProperty); }
                set { SetValue(PasswordProperty, value); }
            }
        }

        public class FolderComboBoxItem : ComboBoxItem
        {
            public static readonly DependencyProperty IndentationProperty = DependencyProperty.Register("Indentation",
                typeof(double), typeof(FolderComboBoxItem));

            public static readonly DependencyProperty EndPathProperty = DependencyProperty.Register("EndPath",
                typeof(Visibility), typeof(FolderComboBoxItem));

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