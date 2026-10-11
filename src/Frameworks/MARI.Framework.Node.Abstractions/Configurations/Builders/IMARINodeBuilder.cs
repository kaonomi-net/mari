// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using Orleans.Hosting;

    /// <summary>
    /// Builder used to configure a cluster node
    /// </summary>
    public interface IMARINodeBuilder : IMARIBuilder<IMARINodeWizard, IMARINodeConfigurationWizard>
    {
        /// <summary>
        /// Call for a manual advance configuration
        /// </summary>
        /// <remarks>
        ///     Could create inappropriate configuration
        /// </remarks>
        ISiloBuilder ManualyAdvancedConfig();
    }
}
