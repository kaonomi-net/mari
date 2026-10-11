// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Extensions.Mongo.Configurations.AutoConfigurator
{
    using MARI.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using MARI.Framework.Configurations;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Configurator used to auto inject mongo configuration when <see cref="AutoConfiguratorKeyAttribute"/> 'mongo' is used
    /// </summary>
    public sealed class AutoMembershipsMongoConfigurator : IMembershipsAutoConfigurator
    {
        /// <inheritdoc />
        public void AutoConfigure(IMARIClusterBuilder mariBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger _)
        {
            var builder = MARIMongoBuilderMARIWizardStartExtensions.GetBuilder(serviceCollection, configuration, mariBuilderWizard);
            builder.SetupMongoCluster(mariBuilderWizard);
        }
    }
}
