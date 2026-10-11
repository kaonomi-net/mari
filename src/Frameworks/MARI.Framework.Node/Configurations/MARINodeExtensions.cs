// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// Maintains Microsoft.Extensions.Hosting to easy fluent setups
// KEEP : Microsoft.Extensions.Hosting
namespace Microsoft.Extensions.Hosting
{
    using MARI.Framework.Cluster.Configurations;
    using MARI.Framework.Configurations;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework;
    using MARI.Framework.Node.Models;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using System;

    /// <summary>
    /// Expose method to activate mari node on existing host
    /// </summary>
    public static class MARINodeExtensions
    {
        /// <summary>
        /// Configure MARI client part in the <paramref name="hostBuilder"/> environment
        /// </summary>
        public static THostBuilder UseMARINode<THostBuilder>(this THostBuilder hostBuilder,
                                                                  Action<IMARINodeBuilder>? builder = null,
                                                                  ClusterBuilderTools? clusterBuilderTools = null)
            where THostBuilder : IHostBuilder
        {
            return UseMARINode<THostBuilder>(hostBuilder, null, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Configure MARI client part in the <paramref name="hostBuilder"/> environment
        /// </summary>
        public static THostBuilder UseMARINode<THostBuilder>(this THostBuilder hostBuilder,
                                                                  Action<HostBuilderContext, IConfigurationBuilder>? setupConfig,
                                                                  Action<IMARINodeBuilder>? builder,
                                                                  ClusterBuilderTools? clusterBuilderTools = null)
            where THostBuilder : IHostBuilder
        {
            MARINode.Create(hostBuilder, setupConfig, builder, clusterBuilderTools);

            hostBuilder.ConfigureServices(services =>
            {
                services.AddHostedService((s) =>
                {
                    return new MARINode(s.GetRequiredService<IHost>(),
                                             s.GetRequiredService<MARINodeConfigurationDefinition>(),
                                             s.GetRequiredService<IMARIExecutionHandler>(),
                                             false);
                });
            });

            return hostBuilder;
        }
    }
}
