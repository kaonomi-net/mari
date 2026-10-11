// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// Define a button with a icon in path format with a text
    /// </summary>
    /// <seealso cref="Button" />
    public class ButtonPath : Button
    {
        #region Fields

        public static readonly DependencyProperty IconPathProperty = DependencyProperty.Register(nameof(IconPath),
                                                                                                 typeof(Geometry),
                                                                                                 typeof(ButtonPath),
                                                                                                 new PropertyMetadata(null));

        public static readonly DependencyProperty IconBrushProperty = DependencyProperty.Register(nameof(IconBrush),
                                                                                                  typeof(Brush),
                                                                                                  typeof(ButtonPath),
                                                                                                  new PropertyMetadata(null));

        public static readonly DependencyProperty IconStyleProperty = DependencyProperty.Register(nameof(IconStyle),
                                                                                                  typeof(Style),
                                                                                                  typeof(ButtonPath),
                                                                                                  new PropertyMetadata(null));

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ButtonPath"/> class.
        /// </summary>
        static ButtonPath()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonPath), new FrameworkPropertyMetadata(typeof(ButtonPath)));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the icon path.
        /// </summary>
        public Geometry IconPath
        {
            get { return (Geometry)GetValue(IconPathProperty); }
            set { SetValue(IconPathProperty, value); }
        }

        /// <summary>
        /// Gets or sets the icon brush.
        /// </summary>
        public Brush IconBrush
        {
            get { return (Brush)GetValue(IconBrushProperty); }
            set { SetValue(IconBrushProperty, value); }
        }

        /// <summary>
        /// Gets or sets the icon style.
        /// </summary>
        public Style? IconStyle
        {
            get { return (Style?)GetValue(IconStyleProperty); }
            set { SetValue(IconStyleProperty, value); }
        }

        #endregion
    }
}
