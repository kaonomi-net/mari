// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Cluster.Abstractions.Configurations.AutoConfigurator
{
    using MARI.Framework.Configurations;

    /// <summary>
    /// Configurator in charge to define how the current cluster part will expose (PORT, GATEWAY ...)
    /// </summary>
    /// <seealso cref="IAutoConfigurator{IMARIClusterBuilder}" />
    public interface IClusterEndpointAutoConfigurator : IAutoConfigurator<IMARIClusterBuilder>
    {
    }
}
