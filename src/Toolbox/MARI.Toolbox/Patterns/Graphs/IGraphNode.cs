// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Graphs
{
    using MARI.Toolbox.Patterns.Graphs.Map;

    /// <summary>
    /// Define a node in a graph
    /// </summary>
    public interface IGraphNode : IEquatable<IGraphNode>
    {
        #region Properties

        /// <summary>
        /// Gets the URI.
        /// </summary>
        /// <remarks>
        ///     Identifier unique by graph
        /// </remarks>
        string Uri { get; }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the properties attached to node
        /// </summary>
        IReadOnlyCollection<GraphProperty> Properties { get; }

        /// <summary>
        /// Gets the <see cref="IReadOnlyCollection{IGraphNode}"/> with the specified relation type to this one.
        /// </summary>
        /// <value>
        IReadOnlyCollection<IGraphNode> this[string relationType] { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is a root node.
        /// </summary>
        bool IsRoot { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has entity.
        /// </summary>
        bool HasEntity { get; }

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        /// Gets the relationships link using this node
        /// </summary>
        IEnumerable<GraphNodeRelation> Relationships { get; }

        /// <summary>
        /// Gets the relationships link from this node.
        /// </summary>
        IReadOnlyCollection<GraphNodeRelation> RelationshipsFrom { get; }

        /// <summary>
        /// Gets the relationships link to this node
        /// </summary>
        IReadOnlyCollection<GraphNodeRelation> RelationshipsTo { get; }

        /// <summary>
        /// Gets the node's types
        /// </summary>
        IReadOnlyCollection<string> Types { get; }

        #endregion

        #region Methods

        /// <inheritdoc cref="IGraphNavigationMap.GetShortestPath(IGraphNode, IGraphNode, string[])" />
        IReadOnlyCollection<IGraphNode> GetShortestPath(IGraphNode target, GetPathOptions? options = null);

        /// <summary>
        /// Gets the related node.
        /// </summary>
        /// <param name="relationTypes">Apply a constraint of allow links</param>
        IReadOnlyCollection<IGraphNode> GetRelatedLink(IReadOnlyCollection<string>? relationTypes = null);

        #endregion
    }

    internal interface IGraphNodeInternal : IGraphNode
    {
        /// <summary>
        /// Sets the graph.
        /// </summary>
        void SetGraph(IGraph graph);

        /// <summary>
        /// Adds node relation.
        /// </summary>
        void AddRelation(GraphNodeRelation inst);
    }
}