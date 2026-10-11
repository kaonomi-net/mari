// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Converters
{
    using MARI.Toolbox.Abstractions.Enums;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable<bool>), typeof(bool))]
    public sealed class LogicBoolAggregatorToBoolConverter : IMultiValueConverter, IValueConverter, ISupportGroupConverter
    {
        #region Fields

        private LogicEnum _logic;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicBoolAggregatorToBoolConverter"/> class.
        /// </summary>
        public LogicBoolAggregatorToBoolConverter()
        {
            this._logic = LogicEnum.And;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the logic.
        /// </summary>
        public LogicEnum Logic
        {
            get { return this._logic; }
            set
            {
                if (value == LogicEnum.And || value == LogicEnum.Or || value == LogicEnum.ExclusiveOr)
                {
                    this._logic = value;
                    return;
                }

                throw new NotSupportedException("Logic value (" + value + ") is not supported by this aggregator only And, Or and ExclusiveOr");
            }
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bValue)
                return ConvertImpl(new[] { bValue }, targetType, parameter, culture);

            if (value is bool[] bValues)
                return ConvertImpl(bValues, targetType, parameter, culture);

            if (value is IEnumerable enumerables && enumerables.OfType<bool>().Any())
                return ConvertImpl(enumerables.OfType<bool>().ToArray(), targetType, parameter, culture);

            if (value is IEnumerable<bool> bValuesEnumerables)
                return ConvertImpl(bValuesEnumerables.ToArray(), targetType, parameter, culture);

            return false;
        }

        /// <inheritdoc />
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is null)
                return Binding.DoNothing;

            // During initialization or on failed bindings MultiBinding could provide DependencyProperty.UnsetValue or null
            if (values.Any(v => v is not bool))
                return Binding.DoNothing;

            return ConvertImpl(values.Cast<bool>().ToArray(), targetType, parameter, culture);
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot reverte bool aggregation");
        }

        /// <inheritdoc />
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot reverte bool aggregation");
        }

        /// <inheritdoc />
        private object ConvertImpl(bool[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is null)
                return Binding.DoNothing;

            return values.OfType<bool>()
                         .Aggregate((bool?)null, (acc, val) =>
                         {
                             if (acc is null)
                                 return val;

                             if (this.Logic == LogicEnum.Or)
                                 return acc | val;

                             if (this.Logic == LogicEnum.ExclusiveOr)
                                 return acc ^ val;

                             return acc & val;
                         }) ?? false;
        }

        #endregion
    }
}
