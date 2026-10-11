// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Extensions.Docker
{
    using MARI.Framework.Cluster.Abstractions.Services;
    using MARI.Framework.Core.Abstractions.Artifacts;
    using MARI.Framework.Extensions.Docker.Abstractions;
    using MARI.Framework.Node.Artifacts;

    using MARI.Toolbox.Abstractions.Services;
    using Microsoft.Extensions.Configuration;

    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class ExternalDockerCodeDaemonExecutor : ExternalCodeDeamonExecutor
    {
        #region Fields

        private readonly IDockerProcessorFactory _dockerProcessorFactory;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalDockerCodeDaemonExecutor"/> class.
        /// </summary>
        public ExternalDockerCodeDaemonExecutor(ArtifactExecutableDefinition artifactExecutableDefinition,
                                                IProcessSystemService processSystemService,
                                                IJsonSerializer jsonSerializer,
                                                INetworkInspector networkInspector,
                                                IDockerProcessorFactory dockerProcessorFactory,
                                                IConfiguration configuration)
            : base(artifactExecutableDefinition, processSystemService, jsonSerializer, networkInspector, configuration, null)
        {
            this._dockerProcessorFactory = dockerProcessorFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override Task<IExternalProcess> LaunchProcess(string executor, List<string> args, ArtifactExecutableDefinition definition, CancellationToken token)
        {
            return this._dockerProcessorFactory.BuildProcessHandler(executor, args, definition, token);
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class ExternalDockerCodeCLIExecutor : ExternalCodeCLIExecutor
    {
        #region Fields

        private readonly IDockerProcessorFactory _dockerProcessorFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalDockerCodeCLIExecutor"/> class.
        /// </summary>
        public ExternalDockerCodeCLIExecutor(ArtifactExecutableDefinition artifactExecutableDefinition,
                                             IProcessSystemService processSystemService,
                                             IJsonSerializer jsonSerializer,
                                             IConfiguration configuration,
                                             IDockerProcessorFactory dockerProcessorFactory) 
            : base(artifactExecutableDefinition, processSystemService, jsonSerializer, configuration, null)
        {
            this._dockerProcessorFactory = dockerProcessorFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override async Task<IExternalProcess> LaunchProcessAsync(string executor, List<string> args, ArtifactExecutableDefinition definition, CancellationToken token)
        {
            var handler = await this._dockerProcessorFactory.BuildProcessHandler(executor, args, definition, token);
            return handler;
        }

        #endregion
    }
}
