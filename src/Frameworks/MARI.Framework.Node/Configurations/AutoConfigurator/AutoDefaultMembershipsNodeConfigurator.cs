// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Configurations.AutoConfigurator
{
    using MARI.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using MARI.Framework.Configurations;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Configurator in charge to setup the default services
    /// </summary>
    /// <seealso cref="IMembershipsAutoConfigurator" />
    public sealed class AutoDefaultMembershipsNodeConfigurator : IMembershipsAutoConfigurator
    {
        /// <inheritdoc />
        public void AutoConfigure(IMARIClusterBuilder mariBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceDescriptors,
                                  ILogger logger)
        {
            var siloBuilder = mariBuilderWizard.GetSiloBuilder();
            siloBuilder.UseLocalhostClustering();
        }
    }
}
