// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Collections
{
    using MARI.Toolbox.Abstractions.Patterns.Pools;
    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Patterns.Pools;

    using Newtonsoft.Json.Linq;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Dictionary used to store information with multiple keys.
    /// Optimize to add and retreived, the count or global loop is slow
    /// </summary>
    public sealed class MultiKeyDictionary<TKey, TValue> : SafeDisposable, IMultiKeyDictionary<TKey, TValue>
        where TKey : notnull
    {
        #region Fields

        private static readonly ThreadSafePool<MultiKeyDictionary<TKey, TValue>.MultiKeyNode> s_pools;

        private readonly Dictionary<TKey, MultiKeyDictionary<TKey, TValue>.MultiKeyNode> _root;

        private readonly IPool<MultiKeyDictionary<TKey, TValue>.MultiKeyNode> _pool;
        private readonly bool _useLocalPool;

        #endregion

        #region Ctor

        /// <summary>
        /// 
        /// </summary>
        static MultiKeyDictionary()
        {
            s_pools = new ThreadSafePool<MultiKeyDictionary<TKey, TValue>.MultiKeyNode>(10_000);
        }

        /// <summary>
        /// Initialize a new instance of the class <see cref="MultiKeyDictionary"/>
        /// </summary>
        public MultiKeyDictionary(int capacity = -1, bool useLocalPool = false)
        {
            this._root = new Dictionary<TKey, MultiKeyNode>();

            this._pool = s_pools;

            if (useLocalPool)
                this._pool = new Pool<MultiKeyDictionary<TKey, TValue>.MultiKeyNode>(150, capacity > 0 ? capacity : null);

            this._useLocalPool = useLocalPool;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public TValue this[IReadOnlyCollection<TKey> keys]
        {
            get
            {
                if (this.TryGetValue(keys, out var value))
                    return value;

                throw new KeyNotFoundException("Key Not Found " + string.Join(", ", keys));
            }

            set { AddImpl(keys, value, true); }
        }

        #endregion

        #region Neested

        private sealed class MultiKeyNode : PoolBaseItem
        {
            #region Fields

            private readonly Dictionary<TKey, MultiKeyDictionary<TKey, TValue>.MultiKeyNode> _children;
            private Func<IEnumerable<TKey>>? _parentKeys;
            private TKey? _key;

            #endregion

            #region Ctor

            /// <summary>
            /// 
            /// </summary>
            public MultiKeyNode()
            {
                this._children = new Dictionary<TKey, MultiKeyNode>();
            }

            #endregion

            #region properties

            public bool HasValue { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public TValue? Value { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int Count
            {
                get { return this._children.Count; }
            }

            #endregion

            #region Methods

            internal void SetKey(Func<IEnumerable<TKey>> parentKeys, TKey key)
            {
                this._key = key;
                this._parentKeys = parentKeys;
            }

            /// <inheritdoc />
            protected override void OnCleanUp()
            {
                this._children.Clear();
                this.Value = default;
                this._key = default;
                this._parentKeys = null;
            }

            /// <inheritdoc cref="IDictionary{TKey, TValue}.Add(TKey, TValue)" />
            internal void Add(in ReadOnlySpan<TKey> keys, TValue? value, bool allowUpdate)
            {
                var key = keys[0];

                MultiKeyNode? node;

                if (!this._children.TryGetValue(key, out node))
                {
                    node = s_pools.GetItem();
                    node.SetKey(GetKeys, key);

                    this._children.Add(key, node);
                }

                if (keys.Length == 1)
                {
                    if (allowUpdate == false && node.HasValue)
                        throw new InvalidOperationException("A value already exist.");

                    node.Value = value;
                    node.HasValue = true;
                }
                else
                {
                    node.Add(keys.Slice(1), value, allowUpdate);
                }
            }

            /// <inheritdoc cref="IDictionary{TKey, TValue}.Remove(TKey)" />
            internal bool Remove(in ReadOnlySpan<TKey> keys)
            {
                var key = keys[0];

                MultiKeyNode? node;

                if (!this._children.TryGetValue(key, out node))
                    return false;

                var result = false;
                if (keys.Length == 1)
                {
                    node.Value = default;
                    node.HasValue = false;
                    result = true;
                }
                else
                {
                    result = node.Remove(keys.Slice(1));
                }

                if (result && node.HasValue == false && node._children.Count == 0)
                {
                    this._children.Remove(key);
                    node.Release();
                }

                return result;
            }

            /// <inheritdoc />
            public void Clear()
            {
                var items = this._children;
                this._children.Clear();

                foreach (var it in items)
                {
                    it.Value.Clear();
                    it.Value.Release();
                }
            }

            /// <inheritdoc cref="IDictionary{TKey, TValue}.TryGetValue(TKey, out TValue)" />
            internal bool TryGetValue(in ReadOnlySpan<TKey> keys, out TValue? value)
            {
                value = default;
                var key = keys[0];

                MultiKeyNode? node;

                if (!this._children.TryGetValue(key, out node))
                    return false;

                if (keys.Length == 1)
                {
                    value = node.Value;
                    return node.HasValue;
                }

                return node.TryGetValue(keys.Slice(1), out value);
            }

            internal IEnumerable<TKey> GetKeys()
            {
                if (this._parentKeys != null)
                {
                    foreach (var key in this._parentKeys())
                        yield return key;
                }

                yield return this._key!;
            }

            /// <summary>
            /// Recurse item fetch
            /// </summary>
            internal IEnumerable<KeyValuePair<IReadOnlyCollection<TKey>, TValue?>> GetItems()
            {
                foreach (var item in this._children)
                {
                    if (item.Value.HasValue)
                        yield return new KeyValuePair<IReadOnlyCollection<TKey>, TValue?>(item.Value.GetKeys().ToArray(), item.Value.Value);

                    foreach (var childItem in item.Value.GetItems())
                        yield return childItem;
                }
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Add(KeyValuePair<IReadOnlyCollection<TKey>, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<IReadOnlyCollection<TKey>, TValue> item)
        {
            if (TryGetValue(item.Key, out var value))
                return EqualityComparer<TValue>.Default.Equals(value, item.Value);
            return false;
        }

        /// <inheritdoc />
        public bool ContainsKey(IReadOnlyCollection<TKey> key)
        {
            return TryGetValue(key, out _);
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<IReadOnlyCollection<TKey>, TValue> item)
        {
            return Remove(item.Key);
        }

        /// <inheritdoc />
        public void Add(IReadOnlyCollection<TKey> keys, TValue value)
        {
            AddImpl(keys, value, false);
        }

        /// <inheritdoc />
        public void Clear()
        {
            var items = this._root;
            this._root.Clear();

            foreach (var it in items)
            {
                it.Value.Clear();
                it.Value.Release();
            }
        }

        /// <inheritdoc />
        public bool Remove(IReadOnlyCollection<TKey> keys)
        {
            if (keys.Any() == false)
                return false;

            var firstKey = keys.First();

            MultiKeyNode? node;

            if (!this._root.TryGetValue(firstKey, out node))
                return false;

            var result = false;
            if (keys.Count == 1)
            {
                node.HasValue = false;
                node.Value = default;
                result = true;
            }
            else
            {
                ReadOnlySpan<TKey> headTail = keys is TKey[] keyArray ? keyArray : keys.ToArray();
                result = node.Remove(headTail.Slice(1));
            }

            if (result && node.HasValue == false && node.Count == 0)
            {
                this._root.Remove(firstKey);
                node.Release();
            }

            return result;
        }

        /// <inheritdoc />
        public bool TryGetValue(IReadOnlyCollection<TKey> keys, [MaybeNullWhen(false)] out TValue value)
        {
            value = default;

            if (keys.Any() == false)
                return false;

            var firstKey = keys.First();

            MultiKeyNode? node;

            if (!this._root.TryGetValue(firstKey, out node))
                return false;

            if (keys.Count == 1)
            {
                value = node.Value;
                return node.HasValue;
            }

            ReadOnlySpan<TKey> headTail = keys is TKey[] keyArray ? keyArray : keys.ToArray();
            return node.TryGetValue(headTail.Slice(1), out value);
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<KeyValuePair<IReadOnlyCollection<TKey>, TValue>> GetItems()
        {
            foreach (var item in this._root)
            {
                if (item.Value.HasValue)
                    yield return new KeyValuePair<IReadOnlyCollection<TKey>, TValue>(item.Value.GetKeys().ToArray(), item.Value.Value!);

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                foreach (var childItem in item.Value.GetItems())
                    yield return childItem;
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            }
        }

        protected override void DisposeBegin()
        {
            if (this._useLocalPool && this._pool is IDisposable disposablePool)
                disposablePool.Dispose();

            base.DisposeBegin();
        }

        #region Tools

        /// <inheritdoc />
        private void AddImpl(IReadOnlyCollection<TKey> keys, TValue value, bool allowUpdate)
        {
            if (keys.Any() == false)
                throw new InvalidDataException("Key must have at least one value");

            var firstKey = keys.First();

            MultiKeyNode? node;

            if (!this._root.TryGetValue(firstKey, out node))
            {
                node = s_pools.GetItem();
                node.SetKey(() => EnumerableHelper<TKey>.Enumerable, firstKey);
                this._root.Add(firstKey, node);
            }

            if (keys.Count == 1)
            {
                if (allowUpdate == false && node.HasValue)
                    throw new InvalidOperationException("A value already exist.");

                node.HasValue = true;
                node.Value = value;
            }
            else
            {
                ReadOnlySpan<TKey> headTail = keys is TKey[] keyArray ? keyArray : keys.ToArray();
                node.Add(headTail.Slice(1), value, allowUpdate);
            }
        }

        #endregion

        #endregion
    }
}
