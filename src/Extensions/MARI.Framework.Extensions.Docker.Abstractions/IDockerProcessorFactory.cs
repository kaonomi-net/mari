// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Extensions.Docker.Abstractions
{
    using MARI.Framework.Core.Abstractions.Artifacts;

    using MARI.Toolbox.Abstractions.Services;

    /// <summary>
    /// Produce <see cref="IExternalProcess"/> using docker
    /// </summary>
    public interface IDockerProcessorFactory
    {
        /// <summary>
        /// Launches the process.
        /// </summary>
        Task<IExternalProcess> BuildProcessHandler(string executor, List<string> args, ArtifactExecutableDefinition definition, CancellationToken token);
    }
}
