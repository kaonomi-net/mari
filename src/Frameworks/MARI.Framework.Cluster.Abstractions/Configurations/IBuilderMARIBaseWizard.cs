// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Base wizard contract root to every other wizard
    /// </summary>
    public interface IBuilderMARIBaseWizard
    {
        /// <summary>
        /// Gets the DPI service descriptors.
        /// </summary>
        IServiceCollection GetServiceCollection();

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        IConfiguration GetConfiguration();
    }
}
