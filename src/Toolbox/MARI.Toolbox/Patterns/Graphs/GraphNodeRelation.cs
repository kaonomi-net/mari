// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Graphs
{
    using MARI.Toolbox.Abstractions.Supports;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Define a relation betwen two <see cref="GraphNode"/>
    /// </summary>
    [DebuggerDisplay("{ToDebugDisplayName()}")]
    public class GraphNodeRelation : IGraphNodeRelation, ISupportDebugDisplayName
    {
        #region Fields

        public const string PARENT = "parent";

        public const string IS_A = "is-a";

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNodeRelation"/> class.
        /// </summary>
        public GraphNodeRelation(IGraphNode? source,
                                 IGraphNode? target,
                                 IEnumerable<GraphProperty>? properties,
                                 string relationType,
                                 bool oneWay,
                                 bool canNavigateTwoWay)
        {
            this.Source = source;
            this.Target = target;

            this.Properties = properties?.ToArray() ?? EnumerableHelper<GraphProperty>.ReadOnlyArray;
            this.RelationType = relationType;

            this.OneWay = oneWay;
            this.CanNavigateTwoWay = canNavigateTwoWay;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public IGraphNode? Source { get; }

        /// <inheritdoc />
        public IGraphNode? Target { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<GraphProperty> Properties { get; }

        /// <inheritdoc />
        public string RelationType { get; }

        /// <inheritdoc />
        public bool OneWay { get; }

        /// <inheritdoc />
        public bool CanNavigateTwoWay { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Froms the definition.
        /// </summary>
        public static GraphNodeRelation FromDefinition<TEntity>(GraphNodeRelationDefinition definition, GraphDefinition<TEntity> graphDefinition, Func<string, IGraphNode?> getNode)
        {
            var source = getNode(definition.UriSource);
            var target = getNode(definition.UriTarget);

            var inst = new GraphNodeRelation(source,
                                             target,
                                             definition.Properties?.Select(p => GraphProperty.FromDefinition(p, graphDefinition)),
                                             definition.RelationType,
                                             definition.OneWay,
                                             definition.CanNavigateTwoWay);

            if (source is not null && source is IGraphNodeInternal internalSource)
                internalSource.AddRelation(inst);

            if (definition.CanNavigateTwoWay == true && target is not null && target is IGraphNodeInternal internalTarget)
                internalTarget.AddRelation(inst);

            return inst;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(GraphNodeRelation a, GraphNodeRelation b)
        {
            return a?.Equals(b) ?? b is null;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator !=(GraphNodeRelation a, GraphNodeRelation b)
        {
            return !(a == b);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is IGraphNodeRelation relation)
                return Equals(relation);

            return false;
        }

        /// <inheritdoc />
        public bool Equals(IGraphNodeRelation? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return (this.Source?.Equals(other.Source) ?? other.Source is null) &&
                   (this.Target?.Equals(other.Target) ?? other.Target is null) &&
                   this.RelationType == other.RelationType &&
                   this.OneWay == other.OneWay &&
                   this.CanNavigateTwoWay == other.CanNavigateTwoWay;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Source,
                                    this.Target,
                                    this.RelationType,
                                    this.OneWay,
                                    this.CanNavigateTwoWay);
        }

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return "[{0}] -- {1} --> [{2}]".WithArguments(this.Source?.Uri, this.RelationType, this.Target?.Uri);
        }

        #endregion
    }
}