// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.DataModel
{
    using Orleans;

    using System.ComponentModel;

    /// <summary>
    /// Data container with level of trust
    /// </summary>
    /// <remarks>
    ///     Previously named ThrustContainer; the alias keeps the serialized identity of the old name.
    /// </remarks>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    [Alias("MARI.Framework.DataModel.ThrustContainer`1")]
    public sealed record class TrustContainer<TData>(TData Data, double TrustLevel);

    /// <summary>
    /// Obsolete misspelled name of <see cref="TrustContainer{TData}"/>.
    /// </summary>
    [Obsolete("Use TrustContainer<TData> instead. This type will be removed in a future version.")]
    [ImmutableObject(true)]
    public sealed record class ThrustContainer<TData>(TData Data, double TrustLevel)
    {
        /// <summary>
        /// Converts to <see cref="TrustContainer{TData}"/>.
        /// </summary>
        public static implicit operator TrustContainer<TData>(ThrustContainer<TData> container)
        {
            return new TrustContainer<TData>(container.Data, container.TrustLevel);
        }

        /// <summary>
        /// Converts from <see cref="TrustContainer{TData}"/>.
        /// </summary>
        public static implicit operator ThrustContainer<TData>(TrustContainer<TData> container)
        {
            return new ThrustContainer<TData>(container.Data, container.TrustLevel);
        }
    }
}