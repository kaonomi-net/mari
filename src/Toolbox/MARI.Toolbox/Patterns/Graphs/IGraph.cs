// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Graphs
{
    using MARI.Toolbox.Patterns.Graphs.Map;

    /// <summary>
    /// Define a graph
    /// </summary>
    public interface IGraph : IEquatable<IGraph>
    {
        #region Properties

        /// <summary>
        /// Gets the graph root URI.
        /// </summary>
        string Uri { get; }

        /// <summary>
        /// Gets the property reference.
        /// </summary>
        IReadOnlyDictionary<string, GraphPropertyDefinition> PropertyRef { get; }

        /// <summary>
        /// Gets or sets the <see cref="IGraphNode"/> for the specified node URI.
        /// </summary>
        IGraphNode? this[string NodeUri] { get; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        IReadOnlyCollection<GraphProperty> Properties { get; }

        /// <summary>
        /// Gets all nodes.
        /// </summary>
        IReadOnlyCollection<IGraphNode> Nodes { get; }

        /// <summary>
        /// Gets the root nodes.
        /// </summary>
        IReadOnlyCollection<IGraphNode> RootNodes { get; }

        /// <summary>
        /// Gets all graph relations.
        /// </summary>
        IReadOnlyCollection<IGraphNodeRelation> Relations { get; }

        /// <summary>
        /// Gets all graph relations where the source or the target couln't be founded
        /// </summary>
        IReadOnlyCollection<IGraphNodeRelation> LostRelations { get; }

        #endregion

        #region Methods

        /// <inheritdoc cref="IGraphNavigationMap.GetShortestPath(IGraphNode, IGraphNode, string[])" />
        IReadOnlyCollection<IGraphNode> GetShortestPath(IGraphNode source, IGraphNode target, GetPathOptions? options = null);

        /// <summary>
        /// Setups the navigation map.
        /// </summary>
        ValueTask SetupNavigationMap(IGraphNavigationMap map);

        #endregion
    }
}