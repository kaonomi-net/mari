// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Graphs
{
    using System.Collections.Generic;

    public interface IGraphNodeRelation : IEquatable<IGraphNodeRelation>
    {
        #region Properties

        /// <summary>
        /// Gets the relation source
        /// </summary>
        IGraphNode? Source { get; }

        /// <summary>
        /// Gets the relation target.
        /// </summary>
        IGraphNode? Target { get; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        IReadOnlyCollection<GraphProperty> Properties { get; }

        /// <summary>
        /// Gets the type of the relation, use as key to categories the relations
        /// </summary>
        string RelationType { get; }

        /// <summary>
        /// Gets a value indicating whether the relation is valid only from <see cref="Source"/> to <see cref="Target"/>
        /// </summary>
        bool OneWay { get; }

        /// <summary>
        /// Gets a value indicating whether this relation can be navigate two way.
        /// </summary>
        /// <remarks>
        ///     Example : A parent relation is OneWay to give a relation direction but not blocking on navigation
        ///
        ///     if <c>true</c>, this relation only appeared in the source objects
        /// </remarks>
        bool CanNavigateTwoWay { get; }

        #endregion
    }
}