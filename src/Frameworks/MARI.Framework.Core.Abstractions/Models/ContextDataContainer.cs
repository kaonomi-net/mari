// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Models
{
    using MARI.Framework.Core.Abstractions.Repositories;
    using MARI.Framework.Core.Abstractions.Surrogates;
    using MARI.Toolbox.Models;

    public interface IContextDataContainer
    {
        /// <summary>
        /// Determines whether the specified type is match.
        /// </summary>
        bool IsMatch(Type type);

        /// <summary>
        /// Determines whether the specified type is match.
        /// </summary>
        bool IsMatch(ConcretBaseType type);

        /// <summary>
        /// Determines whether the specified context data container is match.
        /// </summary>
        bool IsMatch(IContextDataContainer contextDataContainer);

        /// <summary>
        /// Gets the data.
        /// </summary>
        object? GetData(IMARISerializer mariSerializer);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    /// <seealso cref="Models.ContextDataContainer" />
    public sealed class ContextDataContainer<TData> : IContextDataContainer
        where TData : struct
    {
        #region Fields

        private readonly ConcretBaseType _type;
        private readonly byte[] _serializedData;
        private bool _dataLoaded;
        private TData? _data;
        private object? _dataLock;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextDataContainer{TData}"/> class.
        /// </summary>
        public ContextDataContainer(ConcretBaseType type, byte[] data)
        {
            this._type = type;
            this._serializedData = data;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public object? GetData(IMARISerializer mariSerializer)
        {
            return LazyInitializer.EnsureInitialized(ref this._data,
                                                     ref this._dataLoaded,
                                                     ref this._dataLock,
                                                     () => mariSerializer.Deserialize<TData>(this._serializedData));
        }

        /// <inheritdoc />
        public bool IsMatch(Type type)
        {
            return this._type.IsEqualTo(type, true);
        }

        /// <inheritdoc />
        public bool IsMatch(ConcretBaseType type)
        {
            return this._type.Equals(type);
        }

        /// <inheritdoc />
        public bool IsMatch(IContextDataContainer type)
        {
            return type is ContextDataContainer<TData>;
        }

        /// <summary>
        /// Creates the specified data.
        /// </summary>
        public static ContextDataContainer<TData> Create(in TData data, IMARISerializer mariSerializer)
        {
            return new ContextDataContainer<TData>((ConcretType)data.GetType().GetAbstractType(), mariSerializer.SerializeToBinary(data).ToArray());
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this._type.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is ContextDataContainer<TData> container)
            {
                return container._type.Equals((AbstractType?)this._type) &&
                       container._serializedData.SequenceEqual(this._serializedData);
            }

            return false;
        }

        /// <summary>
        /// Converts to surrogate.
        /// </summary>
        public ContextDataContainerSurrogate<TData> ToSurrogate()
        {
            return new ContextDataContainerSurrogate<TData>()
            {
                SerializedData = this._serializedData,
                Type = ConcretBaseTypeConverter.ConvertToSurrogate(this._type),
            };
        }

        #endregion
    }

    [GenerateSerializer]
    public record struct ContextDataContainerSurrogate<TData>(IConcretTypeSurrogate Type, byte[] SerializedData);

    [RegisterConverter]
    public sealed class ContextDataContainerConverter<TData> : IConverter<ContextDataContainer<TData>, ContextDataContainerSurrogate<TData>>
        where TData : struct
    {
        /// <inheritdoc />
        public ContextDataContainer<TData> ConvertFromSurrogate(in ContextDataContainerSurrogate<TData> surrogate)
        {
            return new ContextDataContainer<TData>(ConcretBaseTypeConverter.ConvertFromSurrogate(surrogate.Type), surrogate.SerializedData);
        }

        /// <inheritdoc />
        public ContextDataContainerSurrogate<TData> ConvertToSurrogate(in ContextDataContainer<TData> value)
        {
            return value.ToSurrogate();
        }
    }
}
