// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox
{
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    public interface IAnyTypeContainer
    {
        #region Properties

        /// <summary>
        /// Gets the type of the data.
        /// </summary>
        Type GetDataType();

        /// <summary>
        /// Gets the data.
        /// </summary>
        object? GetData();

        #endregion
    }

    [Serializable]
    [DataContract]
    [ImmutableObject(true)]
    public sealed class AnyTypeContainer<TType> : IAnyType, IAnyTypeContainer
    {
        #region Fields

        private static readonly Type s_ttypeTraits = typeof(TType);

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AnyTypeContainer{TType}"/> class.
        /// </summary>
        public AnyTypeContainer(TType? data)
        {
            this.Data = data;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the data.
        /// </summary>
        [DataMember]
        public TType? Data { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;

            if (object.ReferenceEquals(this, obj)) 
                return true;

            return obj is AnyTypeContainer<TType> container &&
                   (container.Data?.Equals(this.Data) ?? this.Data is null);
        }

        /// <inheritdoc />
        public object? GetData()
        {
            return this.Data;
        }

        /// <inheritdoc />
        public Type GetDataType()
        {
            return s_ttypeTraits;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Data?.GetHashCode() ?? 0;
        }

        #endregion
    }
}
