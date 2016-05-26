using System.Windows;
using System.Windows.Controls;

namespace MockupApplication
{
    /// <summary>
    ///     Tile used to display an account entry
    /// </summary>
    public class AccountTile : Control
    {
        public static readonly DependencyProperty Content1Property =
            DependencyProperty.Register("Content1", typeof(object), typeof(AccountTile),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        public static readonly DependencyProperty Content2Property =
            DependencyProperty.Register("Content2", typeof(object), typeof(AccountTile),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        static AccountTile()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AccountTile),
                new FrameworkPropertyMetadata(typeof(AccountTile)));
        }

        /// <summary>
        ///     Gets or sets the Content1
        /// </summary>
        /// <value>Content1</value>
        public object Content1
        {
            get { return GetValue(Content1Property); }
            set { SetValue(Content1Property, value); }
        }

        /// <summary>
        ///     Gets or sets the Content2
        /// </summary>
        /// <value>Content2</value>
        public object Content2
        {
            get { return GetValue(Content2Property); }
            set { SetValue(Content2Property, value); }
        }
    }
}