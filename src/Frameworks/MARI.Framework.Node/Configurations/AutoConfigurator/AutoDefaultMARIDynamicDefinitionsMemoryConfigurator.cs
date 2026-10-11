// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Configurations.AutoConfigurator
{
    using MARI.Framework.Configurations;
    using MARI.Framework.Node.Abstractions.Configurations.AutoConfigurator;
    using MARI.Framework.Node.Abstractions.Models;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Auto configure - In AppDomain Memory - dynamic definitions
    /// </summary>
    /// <seealso cref="INodeMARIMemoryAutoConfigurator" />
    public sealed class AutoDefaultMARIDynamicDefinitionsMemoryConfigurator : INodeMARIDynamicDefinitionsMemoryAutoConfigurator
    {
        /// <inheritdoc />
        public void AutoConfigure(IMARINodeMemoryBuilder mariBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger)
        {
            var builder = MARIMemoryInLocalConfiguration.GetBuilder(mariBuilderWizard);
            builder.SetupDynamicDefinitionStorage();
        }
    }
}
