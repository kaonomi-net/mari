// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Services
{
    using MARI.Toolbox.Helpers;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// 
    /// </summary>
    public abstract class SelectorService<TKey, TValue> : ISelectorService<TKey, TValue>
        where TKey : IEquatable<TKey>
    {
        #region Fields

        private readonly IReadOnlyDictionary<TKey, TValue> _indexedItems;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectorService{TKey, TValue}"/> class.
        /// </summary>
        protected SelectorService(IEnumerable<ISelectorItem<TKey, TValue>> items)
        {
            this._indexedItems = items?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? DictionaryHelper<TKey, TValue>.ReadOnly;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual TValue? GetValue(in TKey key)
        {
            if (this._indexedItems.TryGetValue(key, out var value))
                return value;

            return OnFallbackValue(key);
        }

        #region Methods

        /// <summary>
        /// Called when [fallback value].
        /// </summary>
        protected virtual TValue? OnFallbackValue(in TKey key)
        {
            throw new KeyNotFoundException(key?.ToString() ?? string.Empty);
        }

        #endregion

        #endregion
    }
}
