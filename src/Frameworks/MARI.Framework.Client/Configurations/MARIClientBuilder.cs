// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Client.Configurations
{
    using MARI.Framework.Client.Model;
    using MARI.Framework.Client.Services;
    using MARI.Framework.Cluster.Abstractions.Services;
    using MARI.Framework.Cluster.Configurations;
    using MARI.Framework.Configurations;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Services;
    using MARI.Framework.Core.Services;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using System;
    using System.Diagnostics;

    /// <see cref="IMARIClientBuilder" /> implementation
    internal sealed class MARIClientBuilder : ClusterBaseBuilder<IMARIClientBuilderWizard, IMARICoreConfigurationWizard, MARIClientConfigurationDefinition>,
                                                   IMARIClientBuilder,
                                                   IMARIWizardStart<IMARIClientBuilderWizard, IMARICoreConfigurationWizard>,
                                                   IMARIClientBuilderWizard,
                                                   IMARICoreConfigurationWizard
    {
        #region Fields

        private readonly INetworkInspector _networkInspector;
        private readonly IClientBuilder _orleanClientBuilder;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MARIClientBuilder"/> class.
        /// </summary>
        public MARIClientBuilder(IHostBuilder host,
                                      IClientBuilder orleanClientBuilder,
                                      HostBuilderContext builderContext,
                                      ClusterBuilderTools clusterBuilderTools)

            : base(host, true, builderContext, clusterBuilderTools)
        {
            ArgumentNullException.ThrowIfNull(clusterBuilderTools.NetworkInspector);

            this._networkInspector = clusterBuilderTools.NetworkInspector;

            this._orleanClientBuilder = orleanClientBuilder;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public sealed override object SourceOrleanBuilder
        {
            get { return this._orleanClientBuilder; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public sealed override IMARIClientBuilderWizard ConfigureLogging(Action<ILoggingBuilder> configureLogging)
        {
            this._orleanClientBuilder.ConfigureServices(collection => collection.AddLogging(configureLogging));
            return this;
        }

        /// <inheritdoc />
        public override IMARIClientBuilderWizard NoCluster(string? serviceId = null, string? clusterId = null, bool useLoopback = true)
        {
            this._orleanClientBuilder.UseLocalhostClustering(serviceId: serviceId ?? "dev", clusterId: clusterId ?? "dev");
            return this;
        }

        /// <inheritdoc />
        public override IServiceCollection GetServiceCollection()
        {
            return this._orleanClientBuilder.Services;
        }

        #region Tools

        /// <inheritdoc />
        protected override void OnFinalizeManualBuildConfigure(ILogger logger)
        {
            var serviceDescriptors = this._orleanClientBuilder.Services;

            serviceDescriptors.AddSingleton<GrainOrleanFactory>(p =>
            {
                var orleanGrainFactory = typeof(MembershipEntry).Assembly.GetTypes().FirstOrDefault(d => d.Name == "GrainFactory" &&
                                                                                                        (d.Attributes & System.Reflection.TypeAttributes.NotPublic) == System.Reflection.TypeAttributes.NotPublic &&
                                                                                                         d.IsAssignableTo(typeof(IGrainFactory)));
                Debug.Assert(orleanGrainFactory != null);
                return new GrainOrleanFactory((IGrainFactory)p.GetRequiredService(orleanGrainFactory));
            });

            serviceDescriptors.AddSingleton<IGrainOrleanFactory>(p => p.GetRequiredService<GrainOrleanFactory>());

            serviceDescriptors.AddSingleton<IVGrainMARISystemProvider, VGrainClientMARISystemProvider>();

            base.OnFinalizeManualBuildConfigure(logger);
        }

        /// <inheritdoc />
        protected override IMARIClientBuilderWizard GetWizard()
        {
            return this;
        }

        /// <inheritdoc />
        protected override MARIClientConfigurationDefinition OnBuild(ILogger _)
        {
            return new MARIClientConfigurationDefinition();
        }

        #endregion

        #endregion
    }
}
