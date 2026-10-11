// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// Maintains Microsoft.Extensions.Hosting to easy fluent setups
namespace Microsoft.Extensions.Hosting
{
    using MARI.Framework;
    using MARI.Framework.Client.Model;
    using MARI.Framework.Cluster.Configurations;
    using MARI.Framework.Configurations;
    using MARI.Framework.Core.Abstractions;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using System;

    /// <summary>
    /// Expose method to activate mari client on existing host
    /// </summary>
    public static class MARIClientExtensions
    {
        /// <summary>
        /// Configure MARI client part in the <paramref name="hostBuilder"/> environment
        /// </summary>
        public static THostBuilder UseMARIClient<THostBuilder>(this THostBuilder hostBuilder,
                                                                    Action<IMARIClientBuilder>? builder = null,
                                                                    ClusterBuilderTools? clusterBuilderTools = null)
            where THostBuilder : IHostBuilder
        {
            return UseMARIClient<THostBuilder>(hostBuilder, null, builder, clusterBuilderTools);
        }

        /// <summary>
        /// Configure MARI client part in the <paramref name="hostBuilder"/> environment
        /// </summary>
        public static THostBuilder UseMARIClient<THostBuilder>(this THostBuilder hostBuilder,
                                                                    Action<HostBuilderContext, IConfigurationBuilder>? setupConfig,
                                                                    Action<IMARIClientBuilder>? builder,
                                                                    ClusterBuilderTools? clusterBuilderTools)
            where THostBuilder : IHostBuilder
        {
            MARIClient.Create(hostBuilder, setupConfig, builder, clusterBuilderTools);

            hostBuilder.ConfigureServices(services =>
            {
                services.AddHostedService((s) =>
                {
                    return new MARIClient(s.GetRequiredService<IHost>(),
                                               s.GetRequiredService<MARIClientConfigurationDefinition>(),
                                               s.GetRequiredService<IMARIExecutionHandler>(),
                                               false);
                });
            });

            return hostBuilder;
        }
    }
}
