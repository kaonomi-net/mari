// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework
{
    using MARI.Framework.Client.Configurations;
    using MARI.Framework.Client.Model;
    using MARI.Framework.Cluster.Configurations;
    using MARI.Framework.Configurations;
    using MARI.Framework.Core.Abstractions;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// MARI cluster's client root
    /// </summary>
    public sealed class MARIClient : MARIBasePart<MARIClientConfigurationDefinition>
    {
        private bool _clientDeco;
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MARIClient"/> class.
        /// </summary>
        internal MARIClient(IHost host,
                                 MARIClientConfigurationDefinition? config,
                                 IMARIExecutionHandler mariExecutionHandler,
                                 bool hostOwned)
            : base(host, config, mariExecutionHandler, hostOwned)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        /// <remarks>
        ///     Override to managed connect/deconnect
        /// </remarks>
        protected override async Task TryStartHostAsync(CancellationToken token)
        {
            if (this._clientDeco)
            {
                var connectionService = this.Services.GetRequiredService<IClusterClient>() as IHostedService;

                await (connectionService?.StartAsync(token) ?? Task.CompletedTask);
                this._clientDeco = false;
                return;
            }

            await base.TryStartHostAsync(token);
        }

        /// <summary>
        /// Prevent HOST to be cleanup to allow multiple restart
        /// </summary>
        protected override async Task TryStopHostAsync(CancellationToken token)
        {
            var connectionService = this.Services.GetRequiredService<IClusterClient>() as IHostedService;

            await (connectionService?.StopAsync(token) ?? Task.CompletedTask);
            this._clientDeco = true;
        }

        /// <inheritdoc />
        protected override async ValueTask DisposeBeginAsync()
        {
            await base.DisposeBeginAsync();
            // Force host to stop on dispose (owned host is disposed by MARIBasePart.DisposeEndAsync)
            await base.TryStopHostAsync(default);
        }

        /// <summary>
        /// Get and setup a new mari cluster client
        /// </summary>
        public static MARIClient Create(string[] args,
                                             Action<IMARIClientBuilder>? builder,
                                             ClusterBuilderTools? clusterBuilderTools = null)
        {
            return Create(args, null, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Get and setup a new mari cluster client
        /// </summary>
        public static MARIClient Create(Action<HostBuilderContext, IConfigurationBuilder>? setupConfig,
                                             Action<IMARIClientBuilder>? builder,
                                             ClusterBuilderTools? clusterBuilderTools = null)
        {
            return Create(null, setupConfig, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Get and setup a new mari cluster client
        /// </summary>
        public static MARIClient Create(Action<IMARIClientBuilder>? builder = null,
                                             ClusterBuilderTools? clusterBuilderTools = null)
        {
            return Create(null, null, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Get and setup a new mari cluster client
        /// </summary>
        public static MARIClient Create(string[]? args,
                                             Action<HostBuilderContext, IConfigurationBuilder>? setupConfig,
                                             Action<IMARIClientBuilder>? builder,
                                             ClusterBuilderTools? clusterBuilderTools)
        {
            var host = CreateAndConfigureHost(args, setupConfig);

            Create(host, setupConfig, builder, clusterBuilderTools);

            var buildedHost = host.Build();

            return new MARIClient(buildedHost,
                                       buildedHost.Services.GetRequiredService<MARIClientConfigurationDefinition>(),
                                       buildedHost.Services.GetRequiredService<IMARIExecutionHandler>(),
                                       true);
        }

        /// <summary>
        /// Get and setup a new mari cluster client
        /// </summary>
        internal static void Create<THostBuilder>(THostBuilder hostBuilder,
                                                  Action<HostBuilderContext, IConfigurationBuilder>? setupConfig = null,
                                                  Action<IMARIClientBuilder>? builder = null,
                                                  ClusterBuilderTools? clusterBuilderTools = null)
            where THostBuilder : IHostBuilder
        {
            if (builder is null)
            {
                builder = cfg => cfg.WizardConfig()
                                    .SetupClusterFromConfig();
            }

            clusterBuilderTools ??= ClusterBuilderTools.Default;

            hostBuilder.UseOrleansClient((ctx, orleanClientBuilder) =>
            {
                var cfgHost = new MARIClientBuilder(hostBuilder,
                                                         orleanClientBuilder,
                                                         ctx,
                                                         clusterBuilderTools);

                builder?.Invoke(cfgHost);

                var mariClientConfigurationDefinition = cfgHost.Build(ctx);

                orleanClientBuilder.Services.AddSingleton(mariClientConfigurationDefinition);
            });
        }

        #endregion
    }
}
