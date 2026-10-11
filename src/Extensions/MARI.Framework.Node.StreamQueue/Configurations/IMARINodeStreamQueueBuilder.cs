// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.StreamQueue.Configurations
{
    using MARI.Framework.Cluster.Abstractions.Configurations.Builders;

    /// <summary>
    /// Builder in charge to configure stream in the node
    /// </summary>
    public interface IMARINodeStreamQueueBuilder 
    {
        #region Properties

        /// <summary>
        /// Gets the gneric builder tool.
        /// </summary>
        IMARIExtensionBuilderTool ConfigurationTools { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Setups a cluster memory stream available only the cluster memory with default container name <see cref="MARI.Framework.Core.Abstractions.Streams.StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
        /// </summary>
        /// <param name="createMemoryPubSubStorage">Get also the default memory storage need for memory stream with key "PubSubStore"; otherwise you will have to provide the needed storage</param>
        IMARINodeStreamQueueBuilder SetupDefaultMARIMemoryStream(bool createMemoryPubSubStorage = true);

        /// <summary>
        /// Setups a cluster memory stream available only the cluster memory
        /// </summary>
        /// <param name="createMemoryPubSubStorage">Get also the default memory storage need for memory stream with key "PubSubStore"; otherwise you will have to provide the needed storage</param>
        IMARINodeStreamQueueBuilder SetupMemoryStream(string streamContainerName, bool createMemoryPubSubStorage = true);

        #endregion
    }
}
