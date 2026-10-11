// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Collections
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="IDictionary{IReadOnlyCollection{TKey, TValue}" />
    public interface IReadOnlyMultiKeyDictionary<TKey, TValue>
    {
        /// <inheritdoc cref="IDictionary{TKey, TValue}.ContainsKey(TKey)" />
        bool ContainsKey(IReadOnlyCollection<TKey> key);

        /// <inheritdoc cref="IDictionary{TKey, TValue}.TryGetValue(TKey, out TValue)" />
        bool TryGetValue(IReadOnlyCollection<TKey> key, [MaybeNullWhen(false)] out TValue value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="IDictionary{IReadOnlyCollection{TKey, TValue}" />
    public interface IMultiKeyDictionary<TKey, TValue> : IReadOnlyMultiKeyDictionary<TKey, TValue>
    {
        /// <inheritdoc cref="IDictionary{TKey, TValue}.Add(TKey, TValue)" />
        TValue this[IReadOnlyCollection<TKey> key] { get; set; }

        /// <inheritdoc cref="IDictionary{TKey, TValue}.Add(TKey, TValue)" />
        void Add(KeyValuePair<IReadOnlyCollection<TKey>, TValue> item);

        /// <inheritdoc cref="IDictionary{TKey, TValue}.Add(TKey, TValue)" />
        void Add(IReadOnlyCollection<TKey> key, TValue value);

        /// <inheritdoc cref="IDictionary{TKey, TValue}.Remove(TKey)" />
        bool Remove(IReadOnlyCollection<TKey> key);

        /// <inheritdoc cref="ICollection{T}.Clear" />
        void Clear();
    }
}
