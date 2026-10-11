// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// KEEP : Microsoft.Extensions.DependencyInjection
namespace Microsoft.Extensions.DependencyInjection
{
    using MARI.Toolbox.Abstractions.Services;
    using MARI.Toolbox.Builders;
    using MARI.Toolbox.Services;

    using Microsoft.Extensions.DependencyInjection.Extensions;

    using System;

    public static class ServiceCollectionSetupExtensions
    {
        /// <summary>
        /// Setups the globalization resources.
        /// </summary>
        public static IServiceCollection SetupGlobalizationResources(this IServiceCollection services, Action<IGlobalizationStringResourceBuilder> resourceBuilder)
        {
            services.TryAddSingleton<IGlobalizationStringResourceProvider, GlobalizationStringResourceProvider>();

            var inst = new GlobalizationStringResourceBuilder(services);
            resourceBuilder(inst);
            return services;
        }
    }
}
