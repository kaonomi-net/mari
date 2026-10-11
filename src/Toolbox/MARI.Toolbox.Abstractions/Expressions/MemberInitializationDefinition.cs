// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Expressions
{
    using MARI.Toolbox.Abstractions.Models;
    using MARI.Toolbox.Models;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Definition that serialize a member initialization
    /// </summary>
    /// <seealso cref="IEquatable{MemberInitializationDefinition}" />
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class MemberInitializationDefinition : IEquatable<MemberInitializationDefinition>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberInitializationDefinition"/> class.
        /// </summary>
        public MemberInitializationDefinition(ConcretType newType,
                                              IEnumerable<ConcretType> inputs,
                                              AbstractMethod? ctor,
                                              IEnumerable<MemberBindingDefinition> bindings)
        {
            this.Ctor = ctor;
            this.Inputs = inputs?.ToArray() ?? Array.Empty<ConcretType>();
            this.NewType = newType;
            this.Bindings = bindings?.ToArray() ?? Array.Empty<MemberBindingDefinition>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ctor.
        /// </summary>
        [DataMember]
        public AbstractMethod? Ctor { get; }

        /// <summary>
        /// Gets the inputs.
        /// </summary>
        [DataMember]
        public IReadOnlyCollection<ConcretType> Inputs { get; }

        /// <summary>
        /// Get type information to create
        /// </summary>
        [DataMember]
        public ConcretType NewType { get; }

        /// <summary>
        /// Gets the bindings.
        /// </summary>
        [DataMember]
        public IReadOnlyCollection<MemberBindingDefinition> Bindings { get; }

        #endregion

        #region Method

        /// <inheritdoc />
        public bool Equals(MemberInitializationDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return (this.Ctor?.Equals(other.Ctor) ?? other.Ctor is null) &&
                   this.NewType.Equals(other.NewType) &&
                   this.Inputs.SequenceEqual(other.Inputs) &&
                   this.Bindings.SequenceEqual(other.Bindings);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is MemberInitializationDefinition memberInitialization)
                return Equals(memberInitialization); 
            return base.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Ctor,
                                    this.NewType,
                                    this.Inputs.Aggregate(0, (acc, i) => acc ^ i.GetHashCode()),
                                    this.Bindings.Aggregate(0, (acc, i) => acc ^ i.GetHashCode()));
        }

        #endregion
    }
}
