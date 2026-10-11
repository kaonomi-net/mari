// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Converters
{
    using System;
    using System.Globalization;

    /// <summary>
    /// 
    /// </summary>
    public interface ISupportGroupConverter
    {
        /// <inheritdoc cref="IValueConverter.Convert"/>
        object Convert(object value, Type targetType, object parameter, CultureInfo culture);
    }
}
