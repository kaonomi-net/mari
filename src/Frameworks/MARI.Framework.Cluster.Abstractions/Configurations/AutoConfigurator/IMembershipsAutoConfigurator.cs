// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Cluster.Abstractions.Configurations.AutoConfigurator
{
    using MARI.Framework.Configurations;

    /// <summary>
    /// Define a service able to automatically configure the membership table for client/node 
    /// </summary>
    /// <seealso cref="IAutoConfigurator" />
    public interface IMembershipsAutoConfigurator : IAutoConfigurator<IMARIClusterBuilder>
    {
    }
}
