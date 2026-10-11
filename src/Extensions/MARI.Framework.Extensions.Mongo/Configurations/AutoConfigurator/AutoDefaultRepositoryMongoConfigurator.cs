// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Extensions.Mongo.Configurations.AutoConfigurator
{
    using MARI.Framework.Configurations;
    using MARI.Framework.Node.Abstractions.Configurations.AutoConfigurator;

    using Microsoft.Extensions.Configuration;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Auto configure the repository storage
    /// </summary>
    /// <seealso cref="INodeCustomRepositoryMemoryAutoConfigurator" />
    public sealed class AutoDefaultRepositoryMongoConfigurator : INodeDefaultRepositoryMemoryAutoConfigurator
    {
        #region Methods

        /// <summary>
        /// Automatics configure mari section.
        /// </summary>
        public void AutoConfigure(IMARINodeMemoryBuilder mariBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger)
        {

            var builder = MARIMongoBuilderMARIWizardStartExtensions.GetBuilder(serviceCollection, configuration, mariBuilderWizard);
            builder.SetupDefaultRepositoryStorage();
        }

        #endregion
    }
}
