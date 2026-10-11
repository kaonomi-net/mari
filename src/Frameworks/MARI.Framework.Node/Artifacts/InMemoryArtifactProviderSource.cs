// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Artifacts
{
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions.Artifacts;
    using MARI.Framework.Node.Abstractions.Artifacts;

    using System;

    /// <summary>
    /// Artefact provider source used to exposed the one record in memory
    /// </summary>
    /// <seealso cref="IArtifactDefinitionProviderSource" />
    public sealed class InMemoryArtifactProviderSource : InMemoryBaseDefinitionProvider<ArtifactDefinition>, IArtifactDefinitionProviderSource
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryArtifactProviderSource"/> class.
        /// </summary>
        public InMemoryArtifactProviderSource(IServiceProvider serviceProvider) 
            : base(serviceProvider)
        {
        }

        #endregion
    }
}
