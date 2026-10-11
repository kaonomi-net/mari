// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Convert <see cref="bool"/> to <see cref="Visibility"/>
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class BoolToVisibilityConveter : IValueConverter, ISupportGroupConverter
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [use hidden].
        /// </summary>
        public bool UseHidden { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bValue)
            {
                return bValue
                          ? Visibility.Visible
                          : (this.UseHidden ? Visibility.Hidden : Visibility.Collapsed);
            }

            return Binding.DoNothing;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
                return visibility == Visibility.Visible;

            return Binding.DoNothing;
        }

        #endregion
    }
}
