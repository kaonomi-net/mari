// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Serializations.ConverterParts
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Validate conditions based on property value
    /// </summary>
    internal sealed class MARIJsonObjectPropertyMatchConverterConditions<TType, TPropValue> : IMARIJsonObjectConverterConditions
    {
        #region Fields

        private readonly IEqualityComparer<TPropValue> _equalityComparer;
        private readonly TPropValue? _expectedValue;
        private readonly string _propertyName;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MARIJsonObjectPropertyMatchConverterConditions{TPropValue}"/> class.
        /// </summary>
        public MARIJsonObjectPropertyMatchConverterConditions(string propertyName, TPropValue? expectedValue, IEqualityComparer<TPropValue> equalityComparer)
        {
            ArgumentNullException.ThrowIfNull(equalityComparer);
            ArgumentNullException.ThrowIfNullOrEmpty(propertyName);

            this._equalityComparer = equalityComparer;
            this._expectedValue = expectedValue;
            this._propertyName = propertyName;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the specified property.
        /// </summary>
        public static MARIJsonObjectPropertyMatchConverterConditions<TType, TPropValue> Build(Expression<Func<TType, TPropValue>> prop, TPropValue? expectedValue, IEqualityComparer<TPropValue>? equalityComparer)
        {
            var propName = (prop.Body as MemberExpression)?.Member.Name;

            if (string.IsNullOrEmpty(propName))
                throw new InvalidDataException("Only property run are allowed");

            return new MARIJsonObjectPropertyMatchConverterConditions<TType, TPropValue>(propName, expectedValue, equalityComparer ?? EqualityComparer<TPropValue>.Default);
        }

        /// <inheritdoc />
        public bool Validate(Func<string, Tuple<bool, object?>> getJsonValue, Type objectType, object? existingValue)
        {
            var propValue = getJsonValue(this._propertyName);

            return propValue.Item1 && propValue.Item2 is TPropValue o && this._equalityComparer.Equals(o, this._expectedValue);
        }

        #endregion
    }
}
