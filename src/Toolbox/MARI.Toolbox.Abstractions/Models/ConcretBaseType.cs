// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Models
{
    using MARI.Toolbox.Abstractions.Models;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.Threading;

    /// <summary>
    /// <see cref="AbstractType"/> representing a final type that could be directly be use
    /// </summary>
    /// <seealso cref="AbstractType" />
    [DataContract]
    [Serializable]
    [ImmutableObject(true)]

    [KnownType(typeof(CollectionType))]
    [KnownType(typeof(ConcretType))]
    [KnownType(typeof(IncompleteType))]
    public abstract class ConcretBaseType : AbstractType
    {
        #region Fields

        private Type? _cachedType;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcretType"/> class.
        /// </summary>
        public ConcretBaseType(string displayName,
                                string? namespaceName,
                                string assemblyQualifiedName,
                                bool isInterface,
                                bool isGenericComposed,
                                AbstractTypeCategoryEnum abstractTypeCategory)
            : base(displayName, namespaceName, abstractTypeCategory)
        {
            this.AssemblyQualifiedName = assemblyQualifiedName;
            this.IsInterface = isInterface;
            this.IsGenericComposed = isGenericComposed;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type <see cref="Type.AssemblyQualifiedName"/>
        /// </summary>
        [DataMember(IsRequired = true)]
        public string AssemblyQualifiedName { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is interface.
        /// </summary>
        [DataMember()]
        public bool IsInterface { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is generic composed.
        /// </summary>
        [DataMember()]
        public bool IsGenericComposed { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public sealed override Type ToType()
        {
            var cached = Volatile.Read(ref this._cachedType);
            if (cached is not null)
                return cached;

            // Benign race: concurrent resolutions produce the same Type, first published value wins
            return Interlocked.CompareExchange(ref this._cachedType, OnTypeResolution(), null) ?? this._cachedType!;
        }

        /// <summary>
        /// Called when to resolved the <see cref="Type"/> based on local information.
        /// </summary>
        protected virtual Type OnTypeResolution()
        {
            //TO-DO from Opus 5.5 Type loaded from serialized string unchecked : AssemblyQualifiedName comes from deserialized data and is passed straight to Type.GetType, so remote payloads can force loading of arbitrary types or assemblies. The lazy _cachedType write is also unsynchronized. Validate against an allow-list or a known-assemblies resolver before resolving.
            return Type.GetType(this.AssemblyQualifiedName, true);
        }

        /// <inheritdoc />
        protected sealed override bool OnEquals(AbstractType other)
        {
            return other is ConcretBaseType otherConcret &&
                   this.AssemblyQualifiedName == otherConcret.AssemblyQualifiedName &&
                   this.IsInterface == otherConcret.IsInterface &&
                   this.IsGenericComposed == otherConcret.IsGenericComposed &&
                   OnConcretEquals(otherConcret);
        }

        /// <inheritdoc cref="AbstractType.OnEquals(AbstractType)" />
        protected abstract bool OnConcretEquals(ConcretBaseType otherConcret);

        /// <inheritdoc />
        protected override object OnGetHashCode()
        {
            return HashCode.Combine(this.AssemblyQualifiedName,
                                    this.IsInterface,
                                    this.IsGenericComposed,
                                    OnConcreteGetHashCode());
        }

        /// <inheritdoc cref="AbstractType.OnGetHashCode" />
        protected abstract object OnConcreteGetHashCode();

        #endregion
    }
}
