// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Extensions
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Services;
    using MARI.Framework.Core.Services;
    using MARI.Framework.Node.Abstractions.Services;
    using MARI.Framework.Node.Services;

    using Microsoft.Extensions.DependencyInjection;

    using System.Diagnostics;
    using System.Linq;

    public static class ClusterRoutingServiceConfigurationExtensions
    {
        /// <summary>
        /// Setups the grain routing services.
        /// </summary>
        public static IServiceCollection SetupGrainRoutingServices(this IServiceCollection serviceDescriptors)
        {
            serviceDescriptors.AddSingleton<GrainOrleanFactory>(p =>
            {
                var orleanGrainFactory = typeof(MembershipEntry).Assembly.GetTypes().FirstOrDefault(d => d.Name == "GrainFactory" &&
                                                                                                        (d.Attributes & System.Reflection.TypeAttributes.NotPublic) == System.Reflection.TypeAttributes.NotPublic &&
                                                                                                         d.IsAssignableTo(typeof(IGrainFactory)));
                Debug.Assert(orleanGrainFactory != null);
                return new GrainOrleanFactory((IGrainFactory)p.GetRequiredService(orleanGrainFactory));
            });

            serviceDescriptors.AddSingleton<IGrainOrleanFactory>(p => p.GetRequiredService<GrainOrleanFactory>());
            serviceDescriptors.AddSingleton<IVGrainMARISystemProvider, VGrainMARISystemProvider>();

            serviceDescriptors.AddSingleton<ISequenceVGrainProviderFactory, SequenceVGrainProviderFactory>();

            serviceDescriptors.AddSingleton<GrainRouteSiloRootService>()
                                            .AddSingleton<IVGrainRouteService>(p => p.GetRequiredService<GrainRouteSiloRootService>())
                                            .AddSingleton<IGrainFactory>(p => new GrainFactoryScoped(p.GetRequiredService<GrainOrleanFactory>().GrainFactory, p.GetRequiredService<IVGrainRouteService>()));

            return serviceDescriptors;
        }
    }
}
