// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Conditions
{
    using Newtonsoft.Json;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.Serialization;

    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
    public sealed class ConditionCallDefinition : ConditionBaseDefinition
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
    {
        #region Fields

        public const string TypeDiscriminator = "call";

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionCallDefinition"/> class.
        /// </summary>
        public ConditionCallDefinition(ConditionBaseDefinition? instance,
                                       string methodName,
                                       IEnumerable<ConditionBaseDefinition>? arguments)
        {
            this.Instance = instance;
            this.MethodName = methodName;
            this.Arguments = arguments?.ToArray() ?? Array.Empty<ConditionBaseDefinition>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance to call the method; could be null on static method
        /// </summary>
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        [DataMember]
        public ConditionBaseDefinition? Instance { get; }

        /// <summary>
        /// Gets the name of the method called
        /// </summary>
        [DataMember]
        public string MethodName { get; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        [DataMember]
        public IReadOnlyCollection<ConditionBaseDefinition> Arguments { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        public static bool operator !=(ConditionCallDefinition x, ConditionCallDefinition y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        public static bool operator ==(ConditionCallDefinition x, ConditionCallDefinition y)
        {
            return x?.Equals(y) ?? y is null;
        }

        /// <inheritdoc />
        protected override bool OnEquals(ConditionBaseDefinition other)
        {
            var call = other as ConditionCallDefinition;

            return call is not null &&
                   (this.Instance?.Equals(call.Instance) ?? call.Instance is null) &&
                   this.MethodName == call.MethodName &&
                   this.Arguments.SequenceEqual(call.Arguments);
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.Instance,
                                    this.MethodName,
                                    this.Arguments.Aggregate(17, (acc, a) => unchecked((acc * 31) + (a?.GetHashCode() ?? 0))));
        }

        #endregion
    }
}
