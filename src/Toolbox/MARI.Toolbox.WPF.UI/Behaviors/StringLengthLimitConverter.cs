// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Behaviors
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// Limit string size
    /// </summary>
    /// <seealso cref="System.Windows.Data.IValueConverter" />
    public sealed class StringLengthLimitConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                var limit = 50;

                if (parameter is int intParams)
                {
                    limit = intParams;
                }
                else if (parameter is string sParam && int.TryParse(sParam, out var parsLimit))
                {
                    limit = parsLimit;
                }

                if (str.Length > limit)
                    return str.Substring(0, limit - 3) + "...";
            }

            return value;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
