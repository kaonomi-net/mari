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
    using System.Reflection;
    using System.Windows.Data;

    /// <summary>
    /// Converter used to sort the input collection based on parameter property
    /// </summary>
    /// <seealso cref="IValueConverter" />
    public sealed class OrderCollectionConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable collection)
            {
                return Sort(collection, parameter as string ?? string.Empty);
            }
            return value;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private IEnumerable Sort(IEnumerable collection, string propName)
        {
            Dictionary<Type, PropertyInfo?>? localPropCache = null;

            return collection.Cast<object>()
                             .Select(c =>
                             {
                                 if (c is not null && !string.IsNullOrEmpty(propName))
                                 {
                                     PropertyInfo? sortProp = null;
                                     var type = c.GetType();

                                     if (localPropCache is null || !localPropCache.TryGetValue(type, out sortProp))
                                     {
                                         localPropCache ??= new Dictionary<Type, PropertyInfo?>();
                                         sortProp = type.GetProperty(propName);
                                         localPropCache.Add(type, sortProp);
                                     }

                                     if (sortProp is not null)
                                         return (sortObj: sortProp.GetValue(c), obj: c);
                                 }

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                                 return (sortObj: null, obj: c);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
                             })
                             .OrderBy(c => c.sortObj)
                             .Select(s => s.obj)
                             .ToArray();

        }
    }
}
