// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Cluster.Abstractions.Configurations.AutoConfigurator
{
    using MARI.Framework.Configurations;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Root definition of a auto configurator
    /// </summary>
    /// <remarks>
    ///     You must inherit only from verion <see cref="IAutoConfigurator{TWizard}"/>
    /// </remarks>
    public interface IAutoConfigurator
    {

    }

    /// <summary>
    /// Root definition of a auto configurator
    /// </summary>
    public interface IAutoConfigurator<TWizard> : IAutoConfigurator
             where TWizard : IBuilderMARIBaseWizard
    {
        /// <summary>
        /// Automatics configure mari section.
        /// </summary>
        void AutoConfigure(TWizard mariBuilderWizard,
                           IConfiguration configuration,
                           IServiceCollection serviceCollection,
                           ILogger logger);
    }

    /// <summary>
    /// Root definition of a auto configurator
    /// </summary>
    public interface IAutoKeyConfigurator<TWizard> : IAutoConfigurator
             where TWizard : IBuilderMARIBaseWizard
    {
        /// <summary>
        /// Automatics configure mari section.
        /// </summary>
        void AutoConfigure(TWizard mariBuilderWizard,
                           IConfiguration configuration,
                           IServiceCollection serviceCollection,
                           ILogger logger,
                           string sourceConfigurationKey,
                           string targetKey);
    }
}
