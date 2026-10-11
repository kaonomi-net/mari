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

    using System;

    /// <summary>
    /// Auto configure - In AppDomain Memory - custom state memory
    /// </summary>
    /// <seealso cref="INodeCustomGrainMemoryAutoConfigurator" />
    public sealed class AutoDefaultCustomGrainMemoryConfigurator : INodeCustomGrainMemoryAutoConfigurator
    {
        #region Methods

        /// <inheritdoc />
        public void AutoConfigure(IMARINodeMemoryBuilder mariBuilderWizard,
                                  IConfiguration configuration,
                                  IServiceCollection serviceCollection,
                                  ILogger logger,
                                  string sourceConfigurationKey,
                                  string targetKey)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(targetKey);

            var customGrainStorageOption = configuration.GetSection(sourceConfigurationKey).Get<DefaultGrainStorageOption>();

            AutoConfigureCustomStorage(mariBuilderWizard,
                                       configuration,
                                       serviceCollection,
                                       logger,
                                       targetKey,
                                       customGrainStorageOption?.BuildReadRepository ?? false);
        }

        /// <inheritdoc />
        public void AutoConfigureCustomStorage(IMARINodeMemoryBuilder mariBuilderWizard,
                                               IConfiguration configuration,
                                               IServiceCollection serviceCollection,
                                               ILogger logger,
                                               string key,
                                               bool buildReadRepository)
        {
            var builder = MARIMemoryInLocalConfiguration.GetBuilder(mariBuilderWizard);
            builder.SetupGrainStateStorage(key, buildReadRepository);
        }

        #endregion
    }
}
