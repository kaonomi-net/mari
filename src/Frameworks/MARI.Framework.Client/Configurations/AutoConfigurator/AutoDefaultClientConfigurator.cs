// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Client.Configurations.AutoConfigurator
{
    using MARI.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using MARI.Framework.Configurations;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    using System;

    /// <summary>
    /// Configurator in charge to setup the default services
    /// </summary>
    /// <seealso cref="IMembershipsAutoConfigurator" />
    public sealed class AutoDefaultClientConfigurator : IMembershipsAutoConfigurator
    {
        /// <inheritdoc />
        public void AutoConfigure(IMARIClusterBuilder mariBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceDescriptors,
                                  ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(mariBuilderWizard);

            // Prever type case before instead on allocating inline variable in condition if it's use outside the scope

#pragma warning disable IDE0019 // Use pattern matching
            var clientBuilder = mariBuilderWizard.SourceOrleanBuilder as IClientBuilder;
#pragma warning restore IDE0019 // Use pattern matching

            if (mariBuilderWizard.IsServerNode || clientBuilder == null)
                throw new InvalidOperationException("The auto configurator must only be used by client side");

            clientBuilder.UseLocalhostClustering();
        }
    }
}
