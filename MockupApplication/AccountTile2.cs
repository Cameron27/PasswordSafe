using System.Windows;
using System.Windows.Controls;

namespace MockupApplication
{
    /// <summary>
    ///     Tile used to display an account entry
    /// </summary>
    public class AccountTile2 : Control
    {
        static AccountTile2()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AccountTile2),
                new FrameworkPropertyMetadata(typeof(AccountTile2)));
        }

        #region Properties

        public static readonly DependencyProperty AccountProperty =
            DependencyProperty.Register("Account", typeof(object), typeof(AccountTile2),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", typeof(object), typeof(AccountTile2),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        public static readonly DependencyProperty EmailProperty =
            DependencyProperty.Register("Email", typeof(object), typeof(AccountTile2),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(object), typeof(AccountTile2),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        public static readonly DependencyProperty UrlProperty =
            DependencyProperty.Register("Url", typeof(object), typeof(AccountTile2),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        public static readonly DependencyProperty NotesProperty =
            DependencyProperty.Register("Notes", typeof(object), typeof(AccountTile2),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        #endregion

        #region Property getters and setters

        /// <summary>
        ///     Gets or sets the AccountProperty property
        /// </summary>
        /// <value>Account</value>
        public object Account
        {
            get { return GetValue(AccountProperty); }
            set { SetValue(AccountProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the UsernameProperty property
        /// </summary>
        /// <value>Username</value>
        public object Username
        {
            get { return GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the EmailProperty property
        /// </summary>
        /// <value>Email</value>
        public object Email
        {
            get { return GetValue(EmailProperty); }
            set { SetValue(EmailProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the PasswordProperty property
        /// </summary>
        /// <value>Password</value>
        public object Password
        {
            get { return GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the UrlProperty property
        /// </summary>
        /// <value>Url</value>
        public object Url
        {
            get { return GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        /// <summary>
        ///     Gets or sets the NotesProperty property
        /// </summary>
        /// <value>Notes</value>
        public object Notes
        {
            get { return GetValue(NotesProperty); }
            set { SetValue(NotesProperty, value); }
        }

        #endregion
    }
}