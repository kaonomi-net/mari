// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Configurations
{
    using MARI.Framework.Core.Abstractions.Artifacts;
    using MARI.Framework.Node.Abstractions.Artifacts;

    /// <summary>
    /// Build in charge to setup artifact resources and how to access it
    /// </summary>
    public interface IMARINodeArtifactsWizard
    {
        ///// <summary>
        ///// Adds selector to choose when multiple sources provide the same artifact.
        ///// </summary>
        //IMARINodeArtifactsWizard AddResourceSelector();

        /// <summary>
        /// Adds the source artefact resources provider.
        /// </summary>
        IMARINodeArtifactsWizard AddSourceProvider<TSource>(TSource singletonInstances)
            where TSource : class, IArtifactDefinitionProviderSource;

        /// <summary>
        /// Adds the source artefact resources provider.
        /// </summary>
        IMARINodeArtifactsWizard AddSourceProvider<TSource>()
            where TSource : class, IArtifactDefinitionProviderSource;

        /// <summary>
        /// Adds an artifact in definition execution.
        /// </summary>
        IMARINodeArtifactsWizard Register(params ArtifactDefinition[] artifactResource);
    }
}
