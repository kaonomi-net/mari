// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Graphs
{
    /// <summary>
    /// Define a property attached to different part of the graph
    /// </summary>
    public class GraphProperty : IEquatable<GraphProperty>
    {
        #region Fields

        public const string NAME = "name";

        public const string LANGUAGE = "lang";

        public const string WEIGHT = "weight";

        public const string SCORE = "score";

        private readonly GraphPropertyDefinition? _refPropDef;
        private readonly string? _displayName;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphProperty"/> class.
        /// </summary>
        public GraphProperty(GraphPropertyDefinition? refPropDef, string? displayName, string value)
        {
            this._refPropDef = refPropDef;
            this._displayName = displayName;
            this.Value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the reference property identifier.
        /// </summary>
        public string? RefPropId
        {
            get { return this._refPropDef?.RefId; }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name
        {
            get { return this._refPropDef?.DisplayName ?? this._displayName ?? "unknown"; }
        }

        /// <summary>
        /// Gets the property value in string format.
        /// </summary>
        public string Value { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Froms the definition.
        /// </summary>
        public static GraphProperty FromDefinition<TEntity>(GraphPropertyValue p, GraphDefinition<TEntity> graphDefinition)
        {
            GraphPropertyDefinition? refPropDef = null;
            if (!string.IsNullOrEmpty(p.RefId) && graphDefinition.RefProperties.TryGetValue(p.RefId, out var cachedRefValue))
                refPropDef = cachedRefValue;

            return new GraphProperty(refPropDef, p.DisplayName, p.Value);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(GraphProperty? a, GraphProperty? b)
        {
            return a?.Equals(b) ?? b is null;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator !=(GraphProperty? a, GraphProperty? b)
        {
            return !(a == b);
        }

        /// <inheritdoc />
        public bool Equals(GraphProperty? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(this, other))
                return true;

            return this.Name == other.Name &&
                   this.RefPropId == other.RefPropId &&
                   this.Value == other.Value;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is GraphProperty grpProp)
                return Equals(grpProp);
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Name, this.Value, this.RefPropId);
        }

        #endregion
    }
}