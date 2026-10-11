// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Configurations.AutoConfigurator
{
    using MARI.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using MARI.Framework.Configurations;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Auto setup default state memory container
    /// </summary>
    /// <seealso cref="IAutoConfigurator{IClusterNodeBuilderMARIMemoryBuilder}" />
    public interface INodeCustomGrainMemoryAutoConfigurator : IAutoKeyConfigurator<IMARINodeMemoryBuilder>
    {
        /// <summary>
        /// Automatics configure mari section.
        /// </summary>
        void AutoConfigureCustomStorage(IMARINodeMemoryBuilder mariBuilderWizard,
                                        IConfiguration configuration,
                                        IServiceCollection serviceCollection,
                                        ILogger logger,
                                        string key,
                                        bool buildReadRepository = false);
    }
}
