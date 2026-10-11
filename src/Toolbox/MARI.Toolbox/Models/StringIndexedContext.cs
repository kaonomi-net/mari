// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Models
{
    using System;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// 
    /// </summary>
    public sealed class StringIndexedContext
    {
        #region Fields

        private readonly FrozenDictionary<char, IndexNode> _rootNodes;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="StringIndexedContext"/> class.
        /// </summary>
        private StringIndexedContext(IDictionary<char, IndexNode> rootNodes)
        {
            this._rootNodes = rootNodes.ToFrozenDictionary();
        }

        #endregion

        #region Nested

        /// <summary>
        /// 
        /// </summary>
        private sealed class IndexNode
        {
            #region Fields

            private readonly FrozenDictionary<char, IndexNode>? _nodes;
            private readonly bool? _include;

            #endregion

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="IndexNode"/> class.
            /// </summary>
            public IndexNode(char value, IDictionary<char, IndexNode>? children, bool? include, IEqualityComparer<char>? comparer)
            {
                this.Value = value;
                this._include = include;
                this._nodes = children?.ToFrozenDictionary(comparer ?? EqualityComparer<char>.Default);
            }

            #endregion

            #region Properties

            /// <summary>
            /// Gets the value.
            /// </summary>
            public char Value { get; }

            #endregion

            #region Methods

            /// <summary>
            /// Found the best match tree node
            /// </summary>
            public bool Search(in ReadOnlySpan<char> source, int lookIndex, int deep, out bool? include, out int? deepFounded)
            {
                include = null;
                deepFounded = null;

                if (lookIndex < source.Length)
                {
                    var nextChar = source[lookIndex];
                    if (this._nodes is not null && this._nodes.TryGetValue(nextChar, out var nextNode))
                    {
                        var searchResult = nextNode.Search(source, lookIndex + 1, deep + 1, out include, out deepFounded);
                        if (searchResult == true)
                            return true;
                    }
                }

                if (this._include != null)
                {
                    include = this._include == true;
                    deepFounded = deep;

                    return true;
                }

                return false;
            }

            #endregion

        }

        #endregion

        #region Methods

        /// <summary>
        /// Searches the specified source.
        /// </summary>
        public bool Search(in ReadOnlySpan<char> source, int lookIndex, out bool? include, out int? deepFounded)
        {
            include = null;
            deepFounded = null;

            var rootChar = source[lookIndex];
            if (this._rootNodes.TryGetValue(rootChar, out var nextNode))
            {
                var searchResult = nextNode.Search(source, lookIndex + 1, 1, out include, out deepFounded);
                if (searchResult == true)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Creates <see cref="StringIndexedContext"> based on separators and exclude values
        /// </summary>
        public static StringIndexedContext Create(IReadOnlyCollection<string> separators, IReadOnlyCollection<string> exclude, IEqualityComparer<char>? comparer = null)
        {
            var indexes = (separators?.Select(s => (Str: s, Include: (bool?)true)) ?? EnumerableHelper<(string, bool?)>.ReadOnlyArray)
                                      .Concat(exclude?.Select(s => (Str: s, Include: (bool?)false)) ?? EnumerableHelper<(string, bool?)>.ReadOnlyArray)
                                      .GroupBy(g => g.Str[0])
                                      .Select(kv => new IndexNode(kv.Key,
                                                                  CreateChildren(kv.Where(v => v.Str.Length > 1).ToArray(), 1, comparer),
                                                                  kv.Where(k => k.Str.Length == 1)
                                                                    .FirstOrDefault().Include,
                                                                  comparer))
                                      .ToDictionary(k => k.Value);

            return new StringIndexedContext(indexes);
        }

        /// <summary>
        /// Creates <see cref="StringIndexedContext"> based on separators and exclude values
        /// </summary>
        private static IDictionary<char, IndexNode>? CreateChildren(IReadOnlyCollection<(string Str, bool? Include)> separators, int index, IEqualityComparer<char>? comparer = null)
        {
            var indexes = separators.Where(s => s.Str.Length > index)
                                    .GroupBy(g => g.Str[index])
                                    .Select(kv => new IndexNode(kv.Key,
                                                                CreateChildren(kv.Where(v => v.Str.Length > index + 1).ToArray(), index + 1, comparer),
                                                                kv.Where(k => k.Str.Length == index + 1)
                                                                  .FirstOrDefault().Include,
                                                                comparer))
                                    .ToDictionary(k => k.Value);

            if (indexes is null || indexes.Count == 0) 
                return null;

            return indexes;
        }

        #endregion
    }
}
