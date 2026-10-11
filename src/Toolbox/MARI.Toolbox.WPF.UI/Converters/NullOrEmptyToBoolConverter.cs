// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Converters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    /// <summary>
    /// <c>True</c> if value is null or empty
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    /// <seealso cref="MARI.Toolbox.WPF.UI.Converters.ISupportGroupConverter" />
    [ValueConversion(typeof(IEnumerable<>), typeof(bool))]
    public sealed class NullOrEmptyToBoolConverter : IValueConverter, ISupportGroupConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return true;

            if (value is string str)
                return string.IsNullOrEmpty(str);

            if (value is Guid id)
                return id == Guid.Empty;

            if (value is IEnumerable collection)
                return !collection.OfType<object>().Any();

            return false;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("NullOrEmptyToBoolConverter Can't conver back");
        }
    }
}
