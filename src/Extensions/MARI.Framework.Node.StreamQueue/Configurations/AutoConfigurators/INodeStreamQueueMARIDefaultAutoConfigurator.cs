// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Configurations.AutoConfigurator
{
    using MARI.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using MARI.Framework.Cluster.Abstractions.Configurations.Builders;
    using MARI.Framework.Core.Abstractions.Streams;

    /// <summary>
    /// Auto configurator used to configure default mari stream container <see cref="StreamQueueDefinition.DEFAULT_STREAM_KEY"/>
    /// </summary>
    /// <seealso cref="IAutoConfigurator{IMARINodeStreamQueueBuilder}" />
    public interface INodeStreamQueueMARIDefaultAutoConfigurator : IAutoKeyConfigurator<IMARIExtensionBuilderTool>
    {
    }
}
