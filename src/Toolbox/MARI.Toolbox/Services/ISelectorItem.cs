// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Services
{
    /// <summary>
    /// An item used in the selector <see cref="ISelectorService{TKey, TValue}"/>
    /// </summary>
    public interface ISelectorItem<TKey, TValue>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Gets the key.
        /// </summary>
        TKey Key { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        TValue Value { get; }
    }
}
