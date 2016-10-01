using System.Windows;
using System.Windows.Controls;

namespace PasswordSafe.GlobalClasses
{
    namespace CustomControls
    {
        public class FolderExpander : Expander
        {
            public static readonly DependencyProperty PathProperty = DependencyProperty.Register("Path", typeof(string),
                typeof(string));

            public static readonly DependencyProperty HasSubFoldersProperty =
                DependencyProperty.Register("HasSubFolders", typeof(bool), typeof(FolderExpander));

            public static readonly DependencyProperty DefaultProperty = DependencyProperty.Register("Default",
                typeof(bool), typeof(FolderExpander));

            public static readonly DependencyProperty IndentationProperty = DependencyProperty.Register("Indentation",
                typeof(double), typeof(FolderExpander));

            public static readonly DependencyProperty IsHighlightedProperty =
                DependencyProperty.Register("IsHighlighted",
                    typeof(bool), typeof(FolderExpander));

            public static readonly DependencyProperty DisplayTopBlackBarProperty =
                DependencyProperty.Register("DisplayTopBlackBar",
                    typeof(bool), typeof(FolderExpander));

            public static readonly DependencyProperty DisplayBottomBlackBarProperty =
                DependencyProperty.Register("DisplayBottomBlackBar", typeof(bool), typeof(FolderExpander));

            public string Path
            {
                get { return (string) GetValue(PathProperty); }
                set { SetValue(PathProperty, value); }
            }

            public bool HasSubFolders
            {
                get { return (bool) GetValue(HasSubFoldersProperty); }
                set { SetValue(HasSubFoldersProperty, value); }
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

            public bool IsHighlighted
            {
                get { return (bool) GetValue(IsHighlightedProperty); }
                set { SetValue(IsHighlightedProperty, value); }
            }

            public bool DisplayTopBlackBar
            {
                get { return (bool) GetValue(DisplayTopBlackBarProperty); }
                set { SetValue(DisplayTopBlackBarProperty, value); }
            }

            public bool DisplayBottomBlackBar
            {
                get { return (bool) GetValue(DisplayBottomBlackBarProperty); }
                set { SetValue(DisplayBottomBlackBarProperty, value); }
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