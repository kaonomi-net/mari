// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Configurations.AutoConfigurator
{
    using MARI.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using MARI.Framework.Configurations;

    /// <summary>
    /// Auto setup default state memory container
    /// </summary>
    /// <seealso cref="IAutoConfigurator{IClusterNodeBuilderMARIMemoryBuilder}" />
    public interface INodeDefaultRepositoryMemoryAutoConfigurator : IAutoConfigurator<IMARINodeMemoryBuilder>
    {
    }
}
