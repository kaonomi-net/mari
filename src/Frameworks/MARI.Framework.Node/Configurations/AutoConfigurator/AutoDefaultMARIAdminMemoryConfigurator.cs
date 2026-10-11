// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Configurations.AutoConfigurator
{
    using MARI.Framework.Configurations;
    using MARI.Framework.Node.Abstractions.Configurations.AutoConfigurator;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Auto configure - In AppDomain Memory - the vgrain state
    /// </summary>
    /// <seealso cref="INodeMARIMemoryAutoConfigurator" />
    public sealed class AutoDefaultMARIAdminMemoryConfigurator : INodeMARIAdminMemoryAutoConfigurator
    {
        /// <inheritdoc />
        public void AutoConfigure(IMARINodeMemoryBuilder mariBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger)
        {
            var builder = MARIMemoryInLocalConfiguration.GetBuilder(mariBuilderWizard);
            builder.SetupMARIAdminStorage();
        }
    }
}
