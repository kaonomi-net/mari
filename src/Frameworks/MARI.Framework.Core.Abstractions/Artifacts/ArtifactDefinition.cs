// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Artifacts
{
    using MARI.Toolbox.Abstractions.Supports;
    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using OrleansCodeGen.Orleans.Serialization;

    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base implementation of <see cref="IArtifactResourceDefinition"/>
    /// </summary>
    /// <seealso cref="IArtifactResourceDefinition" />
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public abstract class ArtifactDefinition : IEquatable<ArtifactDefinition>, ISupportDebugDisplayName, IDefinition, IRefDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactDefinition"/> class.
        /// </summary>
        protected ArtifactDefinition(Guid uid,
                                     Uri refId,
                                     string displayName,
                                     Version? version,
                                     string hash,
                                     DateTime creationOn,
                                     ArtifactTypeEnum artifactType,
                                     DefinitionMetaData? metaData)
        {
            ArgumentNullException.ThrowIfNull(uid);
            ArgumentNullException.ThrowIfNull(refId);
            ArgumentNullException.ThrowIfNull(displayName);
            ArgumentNullException.ThrowIfNull(hash);
            ArgumentNullException.ThrowIfNull(creationOn);

            this.Uid = uid;
            this.DisplayName = displayName;
            this.Version = version;
            this.Hash = hash;
            this.CreationOn = creationOn;
            this.ArtifactType = artifactType;
            this.MetaData = metaData;
            this.RefId = refId;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [DataMember]
        [Id(0)]
        public Guid Uid { get; }

        /// <inheritdoc />
        [DataMember]
        [Id(1)]
        public virtual string DisplayName { get; }

        /// <inheritdoc />
        [DataMember]
        [Id(2)]
        public Version? Version { get; }

        /// <inheritdoc />
        [DataMember]
        [Id(3)]
        public string Hash { get; }

        /// <inheritdoc />
        [DataMember]
        [Id(4)]
        public DateTime CreationOn { get; }

        /// <inheritdoc />
        [DataMember]
        [Id(5)]
        public ArtifactTypeEnum ArtifactType { get; }

        /// <inheritdoc />
        [DataMember]
        [Id(6)]
        public DefinitionMetaData? MetaData { get; }

        /// <inheritdoc />
        [Id(7)]
        [DataMember]
        public Uri RefId { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Equals(ArtifactDefinition? other)
        {
            if (other is null)
                return false;

            if (object.ReferenceEquals(other, this))
                return true;

            return this.Uid == other.Uid &&
                   this.DisplayName == other.DisplayName &&
                   this.Version == other.Version &&
                   this.Hash == other.Hash &&
                   this.CreationOn == other.CreationOn &&
                   this.ArtifactType == other.ArtifactType &&
                   this.MetaData == other.MetaData &&
                   this.RefId == other.RefId &&
                   OnEquals(other);
        }

        /// <inheritdoc cref="IEquatable{ArtifactDefinition}.Equals(ArtifactDefinition?)" />
        protected abstract bool OnEquals([NotNull] ArtifactDefinition other);

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is ArtifactDefinition artifact)
                return Equals(artifact);
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(this.Uid,
                                    this.DisplayName,
                                    this.Version,
                                    this.Hash,
                                    HashCode.Combine(this.CreationOn,
                                                     this.ArtifactType,
                                                     this.MetaData,
                                                     this.RefId,
                                                     OnGetHashCode()));
        }

        /// <inheritdoc cref="object.GetHashCode" />
        protected abstract int OnGetHashCode();

        /// <inheritdoc />
        public virtual string ToDebugDisplayName()
        {
            var description = this.MetaData?.Description;
            if (description is not null && description.Length > 21)
                description = description.Substring(0, 21);

            return $"{this.Uid}-{this.DisplayName}-{this.Version}-{description}";
        }

        /// <inheritdoc />
        public bool Validate(ILogger logger, bool matchWarningAsError = false)
        {
            bool valid = true;

            if (this.Uid == Guid.Empty)
            {
                logger.OptiLog(LogLevel.Error, "Artifact definition Uid must not be empty");
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(this.DisplayName))
            {
                logger.OptiLog(LogLevel.Error, "Artifact definition display name must not be empty");
                valid = false;
            }

            if (this.RefId is null)
            {
                logger.OptiLog(LogLevel.Error, "Artifact definition RefId must not be null");
                valid = false;
            }
            else
            {
                valid &= RefIdHelper.ValidateRefId(this.RefId, logger);
            }

            valid &= OnValidate(logger, matchWarningAsError);
            return valid;
        }

        /// <summary>
        /// Additional validation specific to the artifact implementation
        /// </summary>
        protected virtual bool OnValidate(ILogger logger, bool matchWarningAsError = false)
        {
            return true;
        }

        #endregion
    }
}
