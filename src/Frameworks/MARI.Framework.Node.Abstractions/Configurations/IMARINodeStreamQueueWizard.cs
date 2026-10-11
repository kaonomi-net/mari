// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Configurations
{
    using MARI.Framework.Core.Abstractions.Streams;

    /// <summary>
    /// Build in charge to setup StreamQueue resources and how to access it
    /// </summary>
    public interface IMARINodeStreamQueueWizard
    {
        /// <summary>
        /// Adds an streamQueue in definition execution.
        /// </summary>
        IMARINodeStreamQueueWizard Register(params StreamQueueDefinition[] streamQueueDefinitions);
    }
}
