// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.StreamQueue.Configurations.AutoConfigurators
{
    using MARI.Framework.Cluster.Abstractions.Configurations.Builders;
    using MARI.Framework.Configurations;
    using MARI.Framework.Node.Abstractions.Configurations.AutoConfigurator;
    using MARI.Framework.Node.StreamQueue.Models;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using Orleans.Providers;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Configure memory stream dedicated to default mari stream <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"
    /// </summary>
    /// <seealso cref="INodeStreamQueueMARIDefaultAutoConfigurator" />
    public sealed class AutoMemoryStreamQueueConfigurator : INodeStreamQueueAutoConfigurator
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="AutoMemoryStreamQueueConfigurator"/> class.
        /// </summary>
        static AutoMemoryStreamQueueConfigurator()
        {
            Default = new AutoMemoryStreamQueueConfigurator();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static AutoMemoryStreamQueueConfigurator Default { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Automatics configure mari section.
        /// </summary>
        public void AutoConfigure(IMARIExtensionBuilderTool mariBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger,
                                  string sourceConfigurationKey,
                                  string targetKey)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(targetKey);

            var cfg = configuration.GetSection(sourceConfigurationKey).Get<MemoryStreamQueueOption>();

            AutoConfigureImpl(mariBuilderWizard, logger, new MemoryStreamQueueOption(targetKey, cfg?.CreateInMemoryPubSubStorage ?? true));
        }

        /// <summary>
        /// Automatics the configure implementation.
        /// </summary>
        internal void AutoConfigureImpl(IMARIExtensionBuilderTool mariBuilderWizard,
                                        ILogger logger,
                                        MemoryStreamQueueOption memoryStreamQueueOption)
        {
            var siloBuilder = mariBuilderWizard.GetSiloBuilder();

            // Idea : To be able to store message elsewhere with in memory buffer (mongo, ...) Use MemoryAdapterFactory with a igrain factory redirection to custom IMemoryStreamQueueGrain

            // public class MemoryStreamQueueGrain : Grain, IMemoryStreamQueueGrain, IGrainWithGuidKey, IGrain, IAddressable, IGrainMigrationParticipant
            // siloBuilder.AddPersistentStreams(MemoryAdapterFactory<DefaultMemoryMessageBodySerializer>.Get)

            // https://learn.microsoft.com/en-us/dotnet/orleans/implementation/streams-implementation/

            // UseDynamicClusterConfigDeploymentBalancer -> ensure the stream threads are deployed all around the cluster

            siloBuilder.AddMemoryStreams(memoryStreamQueueOption.ConfigContainerName);//, c => c.UseDynamicClusterConfigDeploymentBalancer());

            if (memoryStreamQueueOption.CreateInMemoryPubSubStorage)
                siloBuilder.AddMemoryGrainStorage("PubSubStore");
        }

        #endregion
    }
}
