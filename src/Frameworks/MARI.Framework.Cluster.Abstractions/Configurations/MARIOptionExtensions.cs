// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    using System.Linq;

    /// <summary>
    /// Extensions method used to help setup option and managed config file, manual, default ...
    /// </summary>
    public static class MARIOptionExtensions
    {
        /// <summary>
        /// Adds option from implementation or instance
        /// </summary>
        /// <returns>
        ///     Return option; null if no option could be determined at registration time
        ///     (ex: option already registered through a factory, resolved later by the service provider)
        /// </returns>
        public static TOption? AddOptionFromInstOrConfig<TOption>(this IServiceCollection services,
                                                                  IConfiguration configuration,
                                                                  string? configurationSection = null,
                                                                  TOption? option = default,
                                                                  bool overrideValue = true)
            where TOption : class
        {
            if (overrideValue == false &&
                (services.Any(s => s.ServiceType == typeof(TOption)) ||
                 services.Any(s => s.ServiceType == typeof(IOptions<TOption>))))
            {
                var serviceDesc = services.FirstOrDefault(s => s.ServiceType == typeof(TOption));

                if (serviceDesc != null)
                {
                    if (serviceDesc.ImplementationInstance is TOption optionInst)
                        return optionInst;

                    // Factory (or type) registrations are resolved by the DI container at runtime;
                    // they MUST NOT be executed at registration time (no service provider available).
                    // The value is unknown here, caller must skip any value dependent setup.
                    if (serviceDesc.ImplementationFactory != null || serviceDesc.ImplementationType != null)
                        return null;

                    throw new InvalidOperationException("Option already registred but could not extract the value");
                }

                serviceDesc = services.FirstOrDefault(s => s.ServiceType == typeof(IOptions<TOption>));

                if (serviceDesc != null)
                {
                    if (serviceDesc.ImplementationInstance is IOptions<TOption> optionIncInst)
                        return optionIncInst.Value;

                    // Same as above : factory registration could not be resolved without a service provider
                    if (serviceDesc.ImplementationFactory != null || serviceDesc.ImplementationType != null)
                        return null;

                    throw new InvalidOperationException("Option already registred but could not extract the value");
                }

                throw new InvalidOperationException("Option already registred but could not extract the value");
            }

            if (!string.IsNullOrWhiteSpace(configurationSection))
            {
                var cfg = configuration.GetSection(configurationSection);

                if (cfg != null && cfg.Exists())
                {
                    services.Configure<TOption>(cfg);

                    var resultMapped = cfg.Get<TOption>();
                    return resultMapped;
                }
            }

            if (option != default)
            {
                services.AddSingleton(option)
                        .AddSingleton(option.ToOption())
                        .AddSingleton(option.ToMonitorOption());

                return option;
            }

            return null;
        }
    }
}
