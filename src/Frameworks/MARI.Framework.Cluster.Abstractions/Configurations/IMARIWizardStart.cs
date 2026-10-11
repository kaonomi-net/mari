// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Simplify entry point of the cluster node configuration wizard
    /// </summary>
    public interface IMARIWizardStart<TWizard, TWizardConfig>
        where TWizard : IMARIWizard<TWizard, TWizardConfig>
        where TWizardConfig : IMARICoreConfigurationWizard<TWizardConfig>
    {
        /// <summary>
        ///  Don't join any cluster, map over the default Orlean local port 1111, May be used for test and dev purpose.
        /// </summary>
        TWizard NoCluster(string? serviceId = null, string? clusterId = null, bool useLoopback = true);

        /// <summary>
        ///  The cluster configuration is only extract from external configuration.
        /// </summary>
        TWizard SetupClusterFromConfig(string? clusterConfigKey = null);

        /// <summary>
        /// Setups the cluster informaiton; Storage system; ... information used to synchonize node and states
        /// </summary>
        TWizard SetupCluster(Action<IMARIClusterBuilder> clusterWizard);

        /// <summary>
        /// Setups the cluster informaiton; Storage system; ... information used to synchonize node and states
        /// </summary>
        TWizard SetupCluster(Action<IMARIClusterBuilder, IConfiguration> clusterWizard);
    }
}
