// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Extensions.Docker.Abstractions.Options
{
    using MARI.Framework.Node.Abstractions.Models;

    using System;
    using System.ComponentModel;

    /// <summary>
    /// Docker environment options
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public class DockerEnvironmentOptions : INodeOptions
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerEnvironmentOptions"/> class.
        /// </summary>
        public DockerEnvironmentOptions()
            : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerEnvironmentOptions"/> class.
        /// </summary>
        public DockerEnvironmentOptions(bool preLoadAllDependenciesImages)
        {
            this.PreLoadAllDependenciesImages = preLoadAllDependenciesImages;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether pre-load all dependencies images at start.
        /// </summary>
        public bool PreLoadAllDependenciesImages { get; }

        #endregion
    }

    /// <summary>
    /// Misspelled legacy name of <see cref="DockerEnvironmentOptions"/>, kept for backward compatibility.
    /// </summary>
    [Obsolete("Use " + nameof(DockerEnvironmentOptions) + " instead.")]
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class DockerEnvironementOptions : DockerEnvironmentOptions
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerEnvironementOptions"/> class.
        /// </summary>
        public DockerEnvironementOptions()
            : base(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerEnvironementOptions"/> class.
        /// </summary>
        public DockerEnvironementOptions(bool preLoadAllDependenciesImages)
            : base(preLoadAllDependenciesImages)
        {
        }

        #endregion
    }
}
