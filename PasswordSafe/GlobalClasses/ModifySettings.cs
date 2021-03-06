﻿#region

using System;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro;

#endregion

namespace PasswordSafe.GlobalClasses
{
    internal static class ModifySettings
    {
        /// <summary>
        ///     Changes the programs AccentColor
        /// </summary>
        /// <param name="accent">Accent to change to</param>
        public static void ChangeProgramsAccent(Accent accent)
        {
            if (accent != null)
            {
                //Used to keep the theme the same 
                Tuple<AppTheme, Accent> currentStyle = ThemeManager.DetectAppStyle(Application.Current);

                ThemeManager.ChangeAppStyle(Application.Current, accent, currentStyle.Item1);
            }
        }

        /// <summary>
        ///     Changes the style of the program
        /// </summary>
        /// <param name="lightMode">True if changing to light mode</param>
        public static void ChangeProgramsTheme(bool lightMode)
        {
            //Used to keep the theme the same 
            Tuple<AppTheme, Accent> currentStyle = ThemeManager.DetectAppStyle(Application.Current);

            ThemeManager.ChangeAppStyle(Application.Current, currentStyle.Item2,
                ThemeManager.GetAppTheme(lightMode ? "BaseLight" : "BaseDark"));
        }

        /// <summary>
        ///     Changes the programs font
        /// </summary>
        /// <param name="font">The font to change to</param>
        public static void ChangeProgramsFont(FontFamily font)
        {
            Application.Current.Resources["MainFont"] = font;
        }

        /// <summary>
        ///     Changes the font size of the program
        /// </summary>
        /// <param name="fontSize">The font size to change to</param>
        public static void ChangeProgramsFontSize(double fontSize)
        {
            Application.Current.Resources["MainFontSize"] = fontSize;
            Application.Current.Resources["MainFontSizePlus2"] = fontSize + 2;
            Application.Current.Resources["MainFontSizePlus2"] = fontSize + 4;
        }
    }
}