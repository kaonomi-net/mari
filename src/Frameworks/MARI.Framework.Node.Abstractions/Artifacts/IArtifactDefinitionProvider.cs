// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Artifacts
{
    using MARI.Framework.Core.Abstractions.Artifacts;
    using MARI.Toolbox.Abstractions.Patterns.Strategy;

    using System;

    /// <summary>
    /// Provider used to get <see cref="ArtifactDefinition"/> from <see cref="IArtifactDefinitionProviderSource"/>
    /// </summary>
    /// <seealso cref="IProviderStrategy{IArtifactResource, Guid}" />
    public interface IArtifactDefinitionProvider : IProviderStrategy<ArtifactDefinition, Guid>
    {
    }
}
