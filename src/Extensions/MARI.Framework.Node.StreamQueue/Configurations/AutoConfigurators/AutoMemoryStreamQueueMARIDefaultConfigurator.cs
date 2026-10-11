// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.StreamQueue.Configurations.AutoConfigurators
{
    using MARI.Framework.Cluster.Abstractions.Configurations.Builders;
    using MARI.Framework.Core.Abstractions.Streams;
    using MARI.Framework.Node.Abstractions.Configurations.AutoConfigurator;
    using MARI.Framework.Node.StreamQueue.Models;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using System;

    /// <summary>
    /// Configure memory stream dedicated to default mari stream <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"
    /// </summary>
    /// <seealso cref="INodeStreamQueueMARIDefaultAutoConfigurator" />
    public sealed class AutoMemoryStreamQueueMARIDefaultConfigurator : INodeStreamQueueMARIDefaultAutoConfigurator
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="AutoMemoryStreamQueueMARIDefaultConfigurator"/> class.
        /// </summary>
        static AutoMemoryStreamQueueMARIDefaultConfigurator()
        {
            Default = new AutoMemoryStreamQueueMARIDefaultConfigurator();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static AutoMemoryStreamQueueMARIDefaultConfigurator Default { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Automatics configure mari section.
        /// </summary>
        public void AutoConfigure(IMARIExtensionBuilderTool mariBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger,
                                  string configurationKey,
                                  string? key)
        {
            AutoConfigureImpl(mariBuilderWizard, logger, true);
        }

        /// <summary>
        /// Automatics configure mari section.
        /// </summary>
        internal void AutoConfigureImpl(IMARIExtensionBuilderTool mariBuilderWizard,
                                        ILogger logger,
                                        bool CreateInMemoryPubSubStorage)
        {
            AutoMemoryStreamQueueConfigurator.Default.AutoConfigureImpl(mariBuilderWizard,
                                                                        logger,
                                                                        new Models.MemoryStreamQueueOption(StreamQueueDefinition.DEFAULT_STREAM_KEY, CreateInMemoryPubSubStorage));
        }

        #endregion
    }
}
