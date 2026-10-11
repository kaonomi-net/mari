// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Models
{
    using MARI.Toolbox.Models;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// <see cref="AbstractType"/> representing a type with generic part that need to be resolved befoer usage
    /// </summary>
    /// <seealso cref="AbstractType" />
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class IncompleteType : ConcretBaseType
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcretType"/> class.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        public IncompleteType(string displayName,
                              string? namespaceName,
                              string assemblyQualifiedName,
                              bool isInterface,
                              IEnumerable<AbstractType> genericParameters)
            : base(displayName,
                   namespaceName,
                   assemblyQualifiedName,
                   isInterface,
                   genericParameters?.Any() ?? false,
                   AbstractTypeCategoryEnum.Incomplet)
        {
            this.GenericParameters = genericParameters?.ToArray() ?? Array.Empty<AbstractType>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the generic parameters if needed.
        /// </summary>
        [DataMember()]
        public IReadOnlyList<AbstractType> GenericParameters { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool OnConcretEquals(ConcretBaseType otherConcret)
        {
            return otherConcret is IncompleteType other &&
                   this.GenericParameters.SequenceEqual(other.GenericParameters);
        }

        /// <inheritdoc />
        protected override object OnConcreteGetHashCode()
        {
            return this.GenericParameters.Aggregate(0, (acc, g) => acc ^ g.GetHashCode());
        }

        #endregion
    }
}
