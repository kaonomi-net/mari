// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Helpers
{
    using System;
    using System.Collections.Generic;

    public sealed class ConvertHelper
    {
        #region Fields
        
        private static readonly IReadOnlyDictionary<Type, Func<string, object>> s_convertFromString;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ConvertHelper"/> class.
        /// </summary>
        static ConvertHelper()
        {
            s_convertFromString = new Dictionary<Type, Func<string, object>>()
            {
                { typeof(Guid), s => Guid.Parse(s) }
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts the value from string.
        /// </summary>
        public static TValue ConvertValueFromString<TValue>(string indexProp)
        {
            return (TValue)ConvertValueFromString(typeof(TValue), indexProp);
        }

        /// <summary>
        /// Converts the value from string.
        /// </summary>
        public static object ConvertValueFromString(Type type, string indexProp)
        {
            if (s_convertFromString.TryGetValue(type, out var builder))
                return builder(indexProp);
            return indexProp;
        }

        #endregion
    }
}
