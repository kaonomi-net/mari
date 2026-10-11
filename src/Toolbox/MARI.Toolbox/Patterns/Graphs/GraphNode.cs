// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Graphs
{
    using MARI.Toolbox.Abstractions.Supports;
    using MARI.Toolbox.Patterns.Graphs.Map;

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;

    /// <summary>
    /// Define a node in a graph link to other by specific relations
    /// </summary>
    [DebuggerDisplay("{ToDebugDisplayName()}")]
    public sealed class GraphNode<TEntity> : IGraphNode, IGraphNodeInternal, ISupportDebugDisplayName
    {
        #region Fields

        private static readonly Type s_traits = typeof(TEntity);

        private readonly List<GraphNodeRelation> _relationFrom;
        private readonly List<GraphNodeRelation> _relationTo;

        private readonly HashSet<IGraphNode> _relatedLink;
        private readonly HashSet<GraphNodeRelation> _relation;

        private readonly List<GraphProperty> _properties;

        private readonly TEntity? _entity;
        private IGraph? _graph;

        #endregion Fields

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNode{TEntity}"/> class.
        /// </summary>
        public GraphNode(string uri,
                         string displayName,
                         bool isRoot,
                         TEntity? entity,
                         IEnumerable<GraphProperty>? properties,
                         IEnumerable<string>? types)
        {
            this.Uri = uri;
            this.DisplayName = displayName;
            this._entity = entity;

            this.IsRoot = isRoot;
            this._properties = new List<GraphProperty>(properties ?? EnumerableHelper<GraphProperty>.ReadOnlyArray);
            this.Properties = new ReadOnlyCollection<GraphProperty>(this._properties);

            this._relation = new HashSet<GraphNodeRelation>();
            this._relatedLink = new HashSet<IGraphNode>();

            this._relationFrom = new List<GraphNodeRelation>();
            this.RelationshipsFrom = new ReadOnlyCollection<GraphNodeRelation>(this._relationFrom);

            this._relationTo = new List<GraphNodeRelation>();
            this.RelationshipsTo = new ReadOnlyCollection<GraphNodeRelation>(this._relationTo);

            this.Types = new SortedSet<string>(types ?? EnumerableHelper<string>.ReadOnlyArray);
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public string Uri { get; }

        /// <inheritdoc />
        public string DisplayName { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<GraphProperty> Properties { get; }

        /// <inheritdoc />
        public bool IsRoot { get; }

        /// <inheritdoc />
        public bool HasEntity
        {
            get { return this._entity is not null; }
        }

        /// <inheritdoc />
        public Type EntityType
        {
            get { return s_traits; }
        }

        /// <inheritdoc />
        public IEnumerable<GraphNodeRelation> Relationships
        {
            get { return this.RelationshipsFrom.Concat(this.RelationshipsTo); }
        }

        /// <inheritdoc />
        public IReadOnlyCollection<GraphNodeRelation> RelationshipsFrom { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<GraphNodeRelation> RelationshipsTo { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<string> Types { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<IGraphNode> this[string relationType]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Build node from it's definitions
        /// </summary>
        public static IGraphNode FromDefinition(GraphNodeDefinition<TEntity> node, GraphDefinition<TEntity> graphDefinition)
        {
            return new GraphNode<TEntity>(node.Uri,
                                          node.DisplayName,
                                          node.IsRoot,
                                          node.Entity,
                                          node.Properties?.Select(p => GraphProperty.FromDefinition(p, graphDefinition)),
                                          node.Types);
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IGraphNode> GetShortestPath(IGraphNode node, GetPathOptions? options = null)
        {
            if (this._graph is null)
                throw new InvalidOperationException("MUST be attached to a graph first");

            return this._graph.GetShortestPath(this, node, options);
        }

        /// <inheritdoc />
        void IGraphNodeInternal.SetGraph(IGraph graph)
        {
            this._graph = graph;
        }

        /// <inheritdoc />
        void IGraphNodeInternal.AddRelation(GraphNodeRelation inst)
        {
            var added = this._relation.Add(inst);

            if (added)
            {
                var from = string.Equals(this.Uri, inst.Source?.Uri);
                var to = string.Equals(this.Uri, inst.Target?.Uri);

                if (from && to)
                    throw new InvalidDataException(string.Format("[Node: {0}][Relation: {1}] Loopback relation are not tolerated, use properties", this.Uri, inst.RelationType));

                if (from)
                {
                    this._relationFrom.Add(inst);

                    if (inst.Target is not null)
                        this._relatedLink.Add(inst.Target);
                }

                if (to)
                {
                    this._relationTo.Add(inst);
                    if (inst.CanNavigateTwoWay && inst.Source is not null)
                        this._relatedLink.Add(inst.Source);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public IReadOnlyCollection<IGraphNode> GetRelatedLink(IReadOnlyCollection<string>? relationTypes = null)
        {
            return this._relatedLink;
        }

        /// <inheritdoc />
        public bool Equals(IGraphNode? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return other is GraphNode<TEntity> otherEntity &&
                   this.Uri == otherEntity.Uri &&
                   this.HasEntity == otherEntity.HasEntity &&
                   this.EntityType == otherEntity.EntityType &&
                   this.IsRoot == otherEntity.IsRoot;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is IGraphNode other)
                return Equals(other);

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Uri.GetHashCode();
        }

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return "{0}".WithArguments(this.Uri);
        }

        #endregion
    }
}