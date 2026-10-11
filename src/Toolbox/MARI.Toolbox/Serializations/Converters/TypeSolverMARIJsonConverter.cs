// Copyright (c) Amexio.

namespace MARI.Toolbox.Converters
{
    using MARI.Toolbox.Serializations;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Apply multiple conditions to founde the correct final type to apply
    /// </summary>
    internal sealed class TypeSolverMARIJsonConverter<TType> : IMARIJsonObjectConverter, IEquatable<TypeSolverMARIJsonConverter<TType>>
    {
        #region Fields

        private static readonly Type s_traits = typeof(TType);

        // Ordered rules : the first matching condition (in registration order) wins
        private readonly KeyValuePair<Type, IMARIJsonObjectConverterConditions>[] _props;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeSolverMARIJsonConverter{TType}"/> class.
        /// </summary>
        public TypeSolverMARIJsonConverter(IReadOnlyDictionary<Type, IMARIJsonObjectConverterConditions> props)
        {
            this._props = props.ToArray();
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool CanWrite
        {
            get { return false; }
        }

        /// <inheritdoc />
        public bool CanRead
        {
            get { return true; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool CanConvert(Type source)
        {
            return s_traits == source;
        }

        /// <inheritdoc />
        public bool ReadJson(Func<string, Tuple<bool, object?>> getJsonValue, ref Type objectType, ref object? existingValue)
        {
            var localObjectType = objectType;
            var localExistingValue = existingValue;

            var propCond = this._props.FirstOrDefault(p => p.Value.Validate(getJsonValue, localObjectType, localExistingValue));

            if (propCond.Key is not null)
                objectType = propCond.Key;

            // Always return false to force use of the new type in classic deserialization
            return false;
        }

        /// <inheritdoc />
        public bool Equals(TypeSolverMARIJsonConverter<TType>? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            return other._props.SequenceEqual(this._props); 
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is TypeSolverMARIJsonConverter<TType> other)
                return Equals(other);
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // Hash computed from the content to stay consistent with Equals
            var hash = new HashCode();
            hash.Add(s_traits);

            foreach (var prop in this._props)
            {
                hash.Add(prop.Key);
                hash.Add(prop.Value);
            }

            return hash.ToHashCode();
        }

        #endregion
    }
}
