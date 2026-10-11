// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Markup;

    [ContentProperty(nameof(GroupConverter.Converters))]
    public sealed class GroupConverter : IValueConverter, ISupportGroupConverter, IMultiValueConverter
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupConverter"/> class.
        /// </summary>
        public GroupConverter()
        {
            this.Converters = new List<ISupportGroupConverter>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the converters.
        /// </summary>
        public List<ISupportGroupConverter> Converters { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var cnv in this.Converters)
                value = cnv.Convert(value, targetType, parameter, culture);

            return value;
        }

        /// <inheritdoc />
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert((object)values, targetType, parameter, culture);
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Grouping cannot ensure conversion is reversible");
        }

        /// <inheritdoc />
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Grouping cannot ensure conversion is reversible");
        }

        #endregion
    }
}
