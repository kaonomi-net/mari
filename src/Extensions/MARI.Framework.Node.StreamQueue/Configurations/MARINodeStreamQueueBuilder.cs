// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.StreamQueue.Configurations
{
    using MARI.Framework.Cluster.Abstractions.Configurations.Builders;
    using MARI.Framework.Node.StreamQueue.Configurations.AutoConfigurators;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Builder in charge to configure stream in the node
    /// </summary>
    internal sealed class MARINodeStreamQueueBuilder : IMARINodeStreamQueueBuilder
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MARINodeStreamQueueBuilder"/> class.
        /// </summary>
        public MARINodeStreamQueueBuilder(IMARIExtensionBuilderTool tool)
        {
            this.ConfigurationTools = tool;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the gneric builder tool.
        /// </summary>
        public IMARIExtensionBuilderTool ConfigurationTools { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Setups a cluster memory stream available only the cluster memory with default container name <see cref="MARI.Framework.Core.Abstractions.Streams.StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </summary>
        /// <param name="createMemoryPubSubStorage">Get also the default memory storage need for memory stream with key "PubSubStore"; otherwise you will have to provide the needed storage</param>
        public IMARINodeStreamQueueBuilder SetupDefaultMARIMemoryStream(bool createMemoryPubSubStorage = true)
        {
            AutoMemoryStreamQueueMARIDefaultConfigurator.Default.AutoConfigureImpl(this.ConfigurationTools, this.ConfigurationTools.Logger, createMemoryPubSubStorage);
            return this;
        }

        /// <summary>
        /// Setups a cluster memory stream available only the cluster memory
        /// </summary>
        /// <param name="createMemoryPubSubStorage">Get also the default memory storage need for memory stream with key "PubSubStore"; otherwise you will have to provide the needed storage</param>
        public IMARINodeStreamQueueBuilder SetupMemoryStream(string streamContainerName, bool createMemoryPubSubStorage = true)
        {
            AutoMemoryStreamQueueConfigurator.Default.AutoConfigureImpl(this.ConfigurationTools, this.ConfigurationTools.Logger, new Models.MemoryStreamQueueOption(streamContainerName, createMemoryPubSubStorage));
            return this;
        }

        #endregion
    }
}
