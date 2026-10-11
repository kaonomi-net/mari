// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Graphs
{
    using MARI.Toolbox.Helpers;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [Serializable]
    [ImmutableObject(true)]
    public record class GraphDefinition<TEntity>(string RootUri,
                                                 IReadOnlyCollection<GraphPropertyDefinition> RefPropertiesDefinitions,
                                                 IReadOnlyCollection<GraphPropertyValue>? Properties,
                                                 IReadOnlyCollection<GraphNodeDefinition<TEntity>> Nodes,
                                                 IReadOnlyCollection<GraphNodeRelationDefinition> Relations,
                                                 IReadOnlyCollection<GraphIndexDefinition> Indexes)
    {
        private IReadOnlyDictionary<string, GraphPropertyDefinition>? _refProperties;

        [IgnoreDataMember]
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public IReadOnlyDictionary<string, GraphPropertyDefinition> RefProperties
        {
            get
            {
                if (this._refProperties is null)
                {
                    this._refProperties = this.RefPropertiesDefinitions?.Where(d => !string.IsNullOrEmpty(d.RefId))
                                                                        .GroupBy(d => d.RefId)
                                                                        .ToDictionary(k => k.Key, v => v.Last()) ?? DictionaryHelper<string, GraphPropertyDefinition>.ReadOnly;
                }
                return this._refProperties;
            }
        }
    }

    public record class GraphDefinition(string RootUri,
                                        IReadOnlyCollection<GraphPropertyDefinition> RefPropertiesDefinitions,
                                        IReadOnlyCollection<GraphPropertyValue>? Properties,
                                        IReadOnlyCollection<GraphNodeDefinition<NoneType>> Nodes,
                                        IReadOnlyCollection<GraphNodeRelationDefinition> Relations,
                                        IReadOnlyCollection<GraphIndexDefinition> Indexes) : GraphDefinition<NoneType>(RootUri, RefPropertiesDefinitions, Properties, Nodes, Relations, Indexes);

    [Serializable]
    [ImmutableObject(true)]
    public sealed record class GraphPropertyDefinition(string RefId,
                                                       string? DisplayName,
                                                       bool OnlyOneAllowed)
    {
        #region Fields

        private const string PROP_REF_PREFIX = "prop_ref_";

        #endregion

        #region Ctor

        /// <summary>
        /// .cctors this instance.
        /// </summary>
        static GraphPropertyDefinition()
        {
            UidProperty = GraphPropertyDefinition.FromDisplayName("Uid");
            SourceProperty = GraphPropertyDefinition.FromDisplayName("Source");
            DisplayNameProperty = GraphPropertyDefinition.FromDisplayName("DisplayName");
            WeightProperty = GraphPropertyDefinition.FromDisplayName("Weight");
            TypeProperty = GraphPropertyDefinition.FromDisplayName("Type");
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the uid property.
        /// </summary>
        public static GraphPropertyDefinition UidProperty { get; }

        /// <summary>
        /// Gets the source property.
        /// </summary>
        public static GraphPropertyDefinition SourceProperty { get; }

        /// <summary>
        /// Gets the display name property.
        /// </summary>
        public static GraphPropertyDefinition DisplayNameProperty { get; }

        /// <summary>
        /// Gets the weight property.
        /// </summary>
        public static GraphPropertyDefinition WeightProperty { get; }

        /// <summary>
        /// Gets the Type property.
        /// </summary>
        public static GraphPropertyDefinition TypeProperty { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Froms the display name.
        /// </summary>
        public static GraphPropertyDefinition FromDisplayName(string displayName, bool onyOneAllowed = true)
        {
            return new GraphPropertyDefinition(BuildPropRefId(displayName.ToLowerInvariant()), displayName, onyOneAllowed);
        }

        /// <summary>
        /// Builds the property reference identifier.
        /// </summary>
        public static string BuildPropRefId(string linkType)
        {
            return PROP_REF_PREFIX + linkType.ToLowerWithSeparator('_')
                                             .Replace("-", "_")
                                             .Replace(" ", "_")
                                             .Replace("é", "e")
                                             .Replace("è", "e")
                                             .Replace("à", "a")
                                             .Replace("ô", "o")
                                             .Replace("û", "u");
        }

        #endregion
    }

    [Serializable]
    [ImmutableObject(true)]
    public sealed record class GraphPropertyValue(string? RefId,
                                                  string? DisplayName,
                                                  string Value);

    [Serializable]
    [ImmutableObject(true)]
    public record class GraphNodeDefinition<TEntity>(string Uri,
                                                     string DisplayName,
                                                     TEntity? Entity,
                                                     IReadOnlyCollection<GraphPropertyValue>? Properties,
                                                     IReadOnlyCollection<string> Types,
                                                     bool IsRoot = false);

    [Serializable]
    [ImmutableObject(true)]
    public record class GraphNodeDefinition(string Uri,
                                            string DisplayName,
                                            IReadOnlyCollection<GraphPropertyValue>? Properties,
                                            IReadOnlyCollection<string> Types,
                                            bool IsRoot = false) : GraphNodeDefinition<NoneType>(Uri, DisplayName, null, Properties, Types, IsRoot);

    [Serializable]
    [ImmutableObject(true)]
    public sealed record class GraphNodeRelationDefinition(string UriSource,
                                                           string UriTarget,
                                                           IReadOnlyCollection<GraphPropertyValue>? Properties,
                                                           string RelationType,
                                                           bool OneWay = true,
                                                           bool CanNavigateTwoWay = true);

    [Serializable]
    [ImmutableObject(true)]
    public abstract record class GraphIndexDefinition(string Uid, string Type);

    [Serializable]
    [ImmutableObject(true)]
    public sealed record class GraphWeightDistanceIndexDefinition(string Uid, IReadOnlyCollection<GraphWeightDistanceDefinition> WeightDistances) : GraphIndexDefinition(Uid, WEIGHT_INDEXED_RELATION)
    {
        public const string WEIGHT_INDEXED_RELATION = "WeightIndexedRelation";
    }

    [Serializable]
    [ImmutableObject(true)]
    public sealed record class GraphWeightDistanceDefinition(string UriSource, string UriTarget, bool Reversible, double Weight);
}