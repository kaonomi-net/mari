// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Graphs
{
    using MARI.Toolbox.Patterns.Graphs.Map;

    using System;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public class Graph<TEntity> : IGraph
    {
        #region Fields

#if NET9_0_OR_GREATER
        private readonly Locker _locker = new Locker();
#else
        private readonly object _locker = new object();
#endif
        private readonly FrozenDictionary<string, IGraphNode> _indexedNodes;
        private IGraphNavigationMap? _graphNavigationMap;

        #endregion Fields

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="Graph{TEntity}"/> class.
        /// </summary>
        public Graph(string uri,
                     IReadOnlyDictionary<string, GraphPropertyDefinition> propertyRef,
                     IEnumerable<GraphProperty>? properties,
                     IEnumerable<IGraphNode>? nodes,
                     IEnumerable<IGraphNodeRelation>? relations,
                     IReadOnlyCollection<GraphIndexDefinition>? indexDefinitions = null,
                     IGraphNavigationMap? graphNavigationMap = null)
        {
            this.Uri = uri;
            this.PropertyRef = propertyRef.ToFrozenDictionary();

            this.Properties = properties?.ToArray() ?? EnumerableHelper<GraphProperty>.ReadOnlyArray;

            this._indexedNodes = (nodes?.ToArray() ?? EnumerableHelper<IGraphNode>.ReadOnlyArray).ToFrozenDictionary(n => n.Uri);

            this.Nodes = this._indexedNodes.Values;
            this.RootNodes = this.Nodes.Where(n => n.IsRoot).ToArray();
            this.Relations = relations?.Where(r => r.Source is not null && r.Target is not null).ToArray() ?? EnumerableHelper<IGraphNodeRelation>.ReadOnlyArray;
            this.LostRelations = relations?.Where(r => r.Source is null || r.Target is null).ToArray() ?? EnumerableHelper<IGraphNodeRelation>.ReadOnlyArray;

            this._graphNavigationMap = graphNavigationMap;

            foreach (var n in this.Nodes.OfType<IGraphNodeInternal>())
                n.SetGraph(this);

            this._graphNavigationMap = graphNavigationMap;

            // TODO : Managed the index correctly
        }

        #endregion Ctor

        #region Properties

        /// <inheritdoc />
        public IReadOnlyDictionary<string, GraphPropertyDefinition> PropertyRef { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<IGraphNode> Nodes { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<IGraphNode> RootNodes { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<IGraphNodeRelation> Relations { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<IGraphNodeRelation> LostRelations { get; }

        /// <inheritdoc />
        public string Uri { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<GraphProperty> Properties { get; }

        /// <inheritdoc />
        public IGraphNode? this[string nodeUri]
        {
            get { return this._indexedNodes.TryGetValueInline(nodeUri, out _); }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Builds a graph from a <see cref="GraphDefinition{TEntity}"/>
        /// </summary>
        public static IGraph BuildFrom(GraphDefinition<TEntity> graphDefinition)
        {
            var nodes = graphDefinition.Nodes
                                       .Select(node => GraphNode<TEntity>.FromDefinition(node, graphDefinition))
                                       .GroupBy(k => k.Uri)
                                       .ToDictionary(k => k.Key, v => v.Last());

            var relations = graphDefinition.Relations
                                           .Select(r => GraphNodeRelation.FromDefinition(r, graphDefinition, uri =>
                                                                                                             {
                                                                                                                 nodes.TryGetValue(uri, out var node);
                                                                                                                 return node;
                                                                                                             }))
                                           .ToArray();

            return new Graph<TEntity>(graphDefinition.RootUri,
                                      graphDefinition.RefProperties,
                                      graphDefinition.Properties?.Select(p => GraphProperty.FromDefinition(p, graphDefinition)),
                                      nodes.Values,
                                      relations,
                                      null);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IGraphNode> GetShortestPath(IGraphNode source, IGraphNode target, GetPathOptions? options = null)
        {
            var map = this._graphNavigationMap;

            if (map is null)
            {
                lock (this._locker)
                {
                    if (this._graphNavigationMap is null)
                        this._graphNavigationMap = DijkstraGraphNavigationMap.BuildFromAsync(this, false).AsTask().GetAwaiter().GetResult();

                    map = this._graphNavigationMap;
                }
            }

            return map.GetShortestPath(source, target, options);
        }

        /// <inheritdoc />
        public ValueTask SetupNavigationMap(IGraphNavigationMap map)
        {
            lock (this._locker)
            {
                this._graphNavigationMap = map;
            }

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public bool Equals(IGraph? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return this.Nodes.SequenceEqual(other.Nodes) &&
                   this.Relations.SequenceEqual(other.Relations) &&
                   OnEquals(other);
        }

        /// <summary>
        /// Called when [equals].
        /// </summary>
        protected virtual bool OnEquals(IGraph other)
        {
            return true;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is IGraph grp)
                return Equals(grp);
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Nodes.Aggregate(0, (acc, n) => acc ^ n.GetHashCode()),
                                    this.Relations.Aggregate(0, (acc, n) => acc ^ n.GetHashCode()));
        }

        #endregion Methods
    }

    public class Graph : Graph<NoneType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Graph"/> class.
        /// </summary>
        public Graph(string uri,
                     IReadOnlyDictionary<string, GraphPropertyDefinition> propertyRef,
                     IEnumerable<GraphProperty>? properties,
                     IEnumerable<IGraphNode>? nodes,
                     IEnumerable<IGraphNodeRelation>? relations,
                     IReadOnlyCollection<GraphIndexDefinition>? indexDefinitions) : base(uri, propertyRef, properties, nodes, relations, indexDefinitions)
        {
        }

        /// <summary>
        /// Builds a graph from a <see cref="GraphDefinition{TEntity}"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IGraph BuildFrom(GraphDefinition graphDefinition)
        {
            return Graph<NoneType>.BuildFrom(graphDefinition);
        }
    }
}