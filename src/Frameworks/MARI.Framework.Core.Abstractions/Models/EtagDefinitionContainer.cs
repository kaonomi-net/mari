// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Models
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Repositories;
    using MARI.Framework.Core.Abstractions.Sequence;

    using MARI.Toolbox.Abstractions.Supports;

    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Container used to save definition with an etag to detect update
    /// </summary>
    /// <typeparam name="TDefinition">The type of the definition.</typeparam>
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public class EtagDefinitionContainer : IEntityWithId<Guid>
    {
        #region Fields
        
        private static readonly MethodInfo s_genericCreateFrom;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="EtagDefinitionContainer"/> class.
        /// </summary>
        static EtagDefinitionContainer()
        {
            Expression<Func<EtagDefinitionContainer<SequenceDefinition>>> expr = () => EtagDefinitionContainer.Create<SequenceDefinition>((SequenceDefinition)null!, (EtagDefinitionContainer)null!);
            s_genericCreateFrom = ((MethodCallExpression)expr.Body).Method.GetGenericMethodDefinition();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="EtagDefinitionContainer"/> class from being created.
        /// </summary>
        public EtagDefinitionContainer(Guid uid,
                                       string etag,
                                       string discriminator)
        {
            this.Uid = uid;
            this.Etag = etag;
            this.Discriminator = discriminator;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the uid.
        /// </summary>
        [DataMember]
        [Id(0)]
        public Guid Uid { get; }

        /// <summary>
        /// Gets or sets the etag.
        /// </summary>
        [DataMember]
        [Id(1)]
        public string Etag { get; }

        /// <summary>
        /// Gets or sets the discriminator.
        /// </summary>
        [DataMember]
        [Id(2)]
        public string Discriminator { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates the specified <see cref="EtagDefinitionContainer"/>.
        /// </summary>
        public static EtagDefinitionContainer<TDefinition> Create<TDefinition>(TDefinition definition)
            where TDefinition : class, IDefinition
        {
            return new EtagDefinitionContainer<TDefinition>(definition);
        }

        /// <summary>
        /// Creates the specified <see cref="EtagDefinitionContainer"/>.
        /// </summary>
        internal static EtagDefinitionContainer<TDefinition> Create<TDefinition>(TDefinition definition, EtagDefinitionContainer source)
            where TDefinition : class, IDefinition
        {
            return new EtagDefinitionContainer<TDefinition>(source.Uid, source.Etag, source.Discriminator, definition);
        }

        /// <summary>
        /// Creates the specified <see cref="EtagDefinitionContainer"/>.
        /// </summary>
        internal static EtagDefinitionContainer CreateFrom(Type definitionType, IDefinition definition, EtagDefinitionContainer source)
        {
            return (EtagDefinitionContainer)s_genericCreateFrom.MakeGenericMethodWithCache(definitionType)
                                                               .Invoke(null, new object?[] { definition, source })!;
        }

        /// <summary>
        /// Test is the definition follow the requirement type
        /// </summary>
        public virtual bool IsDefinition<TDefinition>()
        {
            throw new NotSupportedException("Only DefinitionContainer<TDefinition> could provide the definition");
        }

        /// <summary>
        /// Gets the contain definition.
        /// </summary>
        public virtual TDefinition GetContainDefinition<TDefinition>()
        {
            throw new NotSupportedException("Only DefinitionContainer<TDefinition> could provide the definition");
        }

        #endregion
    }

    /// <summary>
    /// Container used to save definition with an etag to detect update
    /// </summary>
    /// <typeparam name="TDefinition">The type of the definition.</typeparam>
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class EtagDefinitionContainer<TDefinition> : EtagDefinitionContainer, ISupportConvert
        where TDefinition : class, IDefinition
    {
        #region Fields

        private readonly static Type s_genericTraits = typeof(EtagDefinitionContainer<>);
        private readonly static Type s_definitionTraits = typeof(TDefinition);

        private readonly static Newtonsoft.Json.JsonSerializerSettings s_etagSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings()
        {
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
            Formatting = Newtonsoft.Json.Formatting.None,
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
        };

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="EtagDefinitionContainer{TDefinition}"/> class.
        /// </summary>
        static EtagDefinitionContainer()
        {
            DefaultDiscriminator = typeof(TDefinition).GetAbstractType().DisplayName.Replace(".", "").Trim().Replace(" ", "").Replace("-", "").Replace("_", "");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EtagDefinitionContainer{TDefinition}"/> class.
        /// </summary>
        public EtagDefinitionContainer(TDefinition definition)
            : base(definition.Uid,
                   ComputeContentEtag(definition),
                   DefaultDiscriminator)
        {
            this.Definition = definition;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="EtagDefinitionContainer{TDefinition}"/> class from being created.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        internal EtagDefinitionContainer(Guid uid,
                                         string etag,
                                         string discriminator,
                                         TDefinition definition)
            : base(uid, etag, discriminator)
        {
            this.Definition = definition;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the discriminator.
        /// </summary>
        [IgnoreDataMember]
        public static string DefaultDiscriminator { get; }

        /// <summary>
        /// Gets the definition.
        /// </summary>
        [DataMember]
        [Id(0)]
        public TDefinition Definition { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Computes a deterministic etag from the definition content (SHA256 of its JSON serialization).
        /// </summary>
        /// <remarks>
        ///     Falls back to a random etag if the definition could not be serialized.
        /// </remarks>
        private static string ComputeContentEtag(TDefinition definition)
        {
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(definition, s_etagSerializerSettings);
                var payload = Encoding.UTF8.GetBytes(definition.GetType().FullName + "|" + definition.Uid + "|" + json);
                var hash = SHA256.HashData(payload);
                return Convert.ToHexString(hash, 0, 16);
            }
            catch
            {
                return Guid.NewGuid().ToString("N");
            }
        }

        /// <summary>
        /// Converts to containerheaderonly.
        /// </summary>
        public EtagDefinitionContainer ToContainerHeaderOnly()
        {
            return new EtagDefinitionContainer(this.Uid, this.Etag, this.Discriminator);
        }

        /// <summary>
        /// Test is the definition follow the requirement type
        /// </summary>
        public override bool IsDefinition<TRequestDefinition>()
        {
            return this.Definition is TRequestDefinition;
        }

        /// <summary>
        /// Gets the contain definition.
        /// </summary>
        public override TRequestDefinition GetContainDefinition<TRequestDefinition>()
        {
            if (this.Definition is TRequestDefinition requested)
                return requested;

            throw new InvalidCastException($"Could not cast definition type {this.Definition} to requested {typeof(TRequestDefinition)}");
        }

        /// <inheritdoc />
        public bool TryConvert<TTarget>(out TTarget? target)
        {
            if (TryConvert(out var targetObj, typeof(TTarget)))
            {
                target = (TTarget)targetObj!;
                return true;
            }

            target = default;
            return false;
        }

        /// <inheritdoc />
        public bool TryConvert(out object? target, Type targetType)
        {
            target = null;

            if (!targetType.IsGenericType)
                return false;

            var genericTarget = targetType.GetGenericTypeDefinition();
            if (genericTarget != s_genericTraits)
                return false;

            var expectedDefinition = targetType.GetGenericArguments().Single();
            if (this.Definition is not null && this.Definition.GetType().IsAssignableTo(expectedDefinition) == false)
                return false;

            target = EtagDefinitionContainer.CreateFrom(expectedDefinition, this.Definition, this);
            return true;
        }

        #endregion
    }
}
