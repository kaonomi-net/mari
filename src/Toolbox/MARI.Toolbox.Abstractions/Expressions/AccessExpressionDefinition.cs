// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Expressions
{
    using MARI.Toolbox.Abstractions.Models;
    using MARI.Toolbox.Abstractions.Supports;
    using MARI.Toolbox.Models;

    using Newtonsoft.Json;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Converts object properties to their runtime type with type information.
    /// Ensure <c>TypeNameHandling</c> is set to <c>TypeNameHandling.All</c>
    /// </summary>
    public class RuntimeTypeConverter : JsonConverter
    {
        #region Overrides of JsonConverter

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader);
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(object);
        }

        #endregion
    }

    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class AccessExpressionDefinition : IEquatable<AccessExpressionDefinition>, ISupportDebugDisplayName
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessExpressionDefinition"/> class.
        /// </summary>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="directObject">The direct object.</param>
        /// <param name="chainCall">The chain call.</param>
        /// <param name="memberInit">The member initialize.</param>
        public AccessExpressionDefinition(ConcretBaseType targetType,
                                          TypedArgument? directObject,
                                          string? chainCall,
                                          MemberInitializationDefinition? memberInit)
        {
            this.TargetType = targetType;
            this.DirectObject = directObject;
            this.ChainCall = chainCall;
            this.MemberInit = memberInit;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of the target.
        /// </summary>
        [DataMember]
        public ConcretBaseType TargetType { get; }

        /// <summary>
        /// Gets the direct object.
        /// </summary>
        [DataMember]
        public TypedArgument? DirectObject { get; }

        /// <summary>
        /// Gets the chain call.
        /// </summary>
        [DataMember]
        public string? ChainCall { get; }

        /// <summary>
        /// Gets the member initialize.
        /// </summary>
        [DataMember]
        public MemberInitializationDefinition? MemberInit { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            if (this.MemberInit is not null)
                return $"Access {this.TargetType.DisplayName} " + this.MemberInit;

            if (string.IsNullOrEmpty(this.ChainCall))
                return $"Access {this.TargetType.DisplayName} ChainCall : " + this.ChainCall;

            return $"Access {this.TargetType.DisplayName} Direct : " + this.DirectObject;

        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is AccessExpressionDefinition other)
                return Equals(other);
            return false;
        }

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return HashCode.Combine(this.TargetType,
                                    this.DirectObject,
                                    this.ChainCall,
                                    this.MemberInit);
        }

        /// <inheritdoc />
        public bool Equals(AccessExpressionDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return string.Equals(this.ChainCall, other.ChainCall) &&
                   this.TargetType.Equals(other.TargetType) &&
                   this.DirectObject == other.DirectObject &&
                   (this.MemberInit?.Equals(other.MemberInit) ?? other.MemberInit is null);
        }

        #endregion
    }
}
