// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// KEEP : MARI.Framework
namespace MARI.Framework
{
    using MARI.Framework.Cluster.Configurations;
    using MARI.Framework.Configurations;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Node.Models;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;

    using Orleans.Runtime;

    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// MARI cluster's node root
    /// </summary>
    public sealed class MARINode : MARIBasePart<MARINodeConfigurationDefinition>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MARINode"/> class.
        /// </summary>
        internal MARINode(IHost host,
                               MARINodeConfigurationDefinition? config,
                               IMARIExecutionHandler? mariExecutionHandler,
                               bool hostOwned)
            : base(host, config, mariExecutionHandler, hostOwned)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override Task OnStartAsync(CancellationToken token = default)
        {
            var option = this.Services.GetService<IOptions<ClusterNodeOptions>>();

            if (option?.Value.AddSiloInformationToConsole ?? false)
            {
                var silo = this.Services.GetRequiredService<ILocalSiloDetails>();
                Console.Title = silo.Name + " Cl. Id: " + silo.ClusterId;
            }

            return base.OnStartAsync(token);
        }

        /// <inheritdoc />
        /// <remarks>
        /// ATTENTION COULD NOT BE Restart, dispose the node and create new
        /// </remarks>
        protected override Task OnStopAsync(CancellationToken token)
        {
            return base.OnStopAsync(token);
        }

        /// <summary>
        /// Get and setup a new mari cluster node
        /// </summary>
        public static MARINode Create(string[] args,
                                           Action<IMARINodeBuilder>? builder,
                                           ClusterBuilderTools? clusterBuilderTools = null)
        {
            return Create(args, null, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Get and setup a new mari cluster node
        /// </summary>
        public static MARINode Create(Action<HostBuilderContext, IConfigurationBuilder>? setupConfig,
                                           Action<IMARINodeBuilder>? builder,
                                           ClusterBuilderTools? clusterBuilderTools = null)
        {
            return Create((string[]?)null, setupConfig, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Get and setup a new mari cluster node
        /// </summary>
        public static MARINode Create(Action<IMARINodeBuilder>? builder = null,
                                           ClusterBuilderTools? clusterBuilderTools = null)
        {
            return Create((string[]?)null, null, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Get and setup a new mari cluster node
        /// </summary>
        /// <remarks>
        ///     Code on section node part ClusterClient.Get is very similar.
        ///     Couldn't factorize for know due method like UseOrleans that are extension method.
        /// </remarks>
        public static MARINode Create(string[]? args,
                                           Action<HostBuilderContext, IConfigurationBuilder>? setupConfig,
                                           Action<IMARINodeBuilder>? builder,
                                           ClusterBuilderTools? clusterBuilderTools = null)
        {
            var host = CreateAndConfigureHost(args, setupConfig);

            Create(host, setupConfig, builder, clusterBuilderTools);

            var buildedHost = host.Build();

            return new MARINode(buildedHost,
                                     buildedHost.Services.GetRequiredService<MARINodeConfigurationDefinition>(),
                                     buildedHost.Services.GetRequiredService<IMARIExecutionHandler>(),
                                     true);
        }

        /// <summary>
        /// Get and setup a new mari cluster node
        /// </summary>
        /// <remarks>
        ///     Code on section node part ClusterClient.Get is very similar.
        ///     Couldn't factorize for know due method like UseOrleans that are extension method.
        /// </remarks>
        internal static void Create(IHostBuilder host,
                                    Action<HostBuilderContext, IConfigurationBuilder>? setupConfig,
                                    Action<IMARINodeBuilder>? builder,
                                    ClusterBuilderTools? clusterBuilderTools = null)
        {
            if (builder is null)
            {
                builder = cfg => cfg.WizardConfig()
                                    .SetupClusterFromConfig();
            }

            clusterBuilderTools ??= ClusterBuilderTools.Default;

            host.UseOrleans((ctx, orleanBuilder) =>
            {
                var cfgHost = new MARINodeBuilder(host,
                                                       orleanBuilder,
                                                       ctx,
                                                       clusterBuilderTools);
                builder?.Invoke(cfgHost);

                var mariNodeConfigurationDefinition = cfgHost.Build(ctx);

                orleanBuilder.Services.AddSingleton(mariNodeConfigurationDefinition);
            });
        }

        #endregion
    }
}