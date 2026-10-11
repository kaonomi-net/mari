// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Resources
{
    using System.Windows;

    public static class MARIResources
    {
        #region Ctor        

        /// <summary>
        /// Initializes the <see cref="MARIResources"/> class.
        /// </summary>
        static MARIResources()
        {
            Global = new Uri("pack://application:,,,/MARI.Toolbox.WPF.UI;component/Resources/Global.xaml");
            Converter = new Uri("pack://application:,,,/MARI.Toolbox.WPF.UI;component/Resources/Converters.xaml");
            DarkTheme = new Uri("pack://application:,,,/MARI.Toolbox.WPF.UI;component/Resources/Themes/Dark/DarkThemeResources.xaml");
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the global.
        /// </summary>
        public static Uri Global { get; }

        /// <summary>
        /// Gets the converter.
        /// </summary>
        public static Uri Converter { get; }

        /// <summary>
        /// Gets the dark theme.
        /// </summary>
        public static Uri DarkTheme { get; }

        #endregion
    }
}
