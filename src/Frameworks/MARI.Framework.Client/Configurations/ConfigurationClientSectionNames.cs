// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Client.Configurations
{
    using MARI.Framework.Client.Abstractions.Models;
    using MARI.Framework.Configurations;

    /// <summary>
    /// Define the configuration section name used by mari's node
    /// </summary>
    public static class ConfigurationClientSectionNames
    {
        /// <summary>
        /// The endpoints section name configuration
        /// </summary>
        public const string Cluster = ConfigurationSectionNames.Root +
                                      ConfigurationSectionNames.SectionSeparator +
                                      ClusterClientOptions.ConfigurationName;
    }
}
