// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Configurations.AutoConfigurator
{
    using MARI.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using MARI.Framework.Configurations;

    /// <summary>
    /// Auto setup default definition provider
    /// </summary>
    /// <seealso cref="IAutoConfigurator{IClusterNodeBuilderMARIMemoryBuilder}" />
    public interface INodeCustomDefinitionProviderAutoConfigurator : IAutoKeyConfigurator<IMARINodeMemoryBuilder>
    {
        ///// <summary>
        ///// Automatics configure mari section.
        ///// </summary>
        //void AutoConfigureCustomProvider(IMARINodeMemoryBuilder mariBuilderWizard,
        //                                 IConfiguration configuration,
        //                                 IServiceCollection serviceCollection,
        //                                 ILogger logger,
        //                                 string key);
    }
}
