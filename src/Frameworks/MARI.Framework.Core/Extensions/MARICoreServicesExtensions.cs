// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Extensions
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Attributes;
    using MARI.Framework.Core.Abstractions.Diagnostics;
    using MARI.Framework.Core.Abstractions.Signals;
    using MARI.Framework.Core.Abstractions.Storages;
    using MARI.Framework.Core.Attributes;
    using MARI.Framework.Core.Diagnostics;
    using MARI.Framework.Core.Services;
    using MARI.Framework.Core.Signals;
    using MARI.Framework.Core.Storages;

    using MARI.Toolbox.Abstractions.Patterns.Workers;
    using MARI.Toolbox.Abstractions.Services;
    using MARI.Toolbox.Patterns.Workers;
    using MARI.Toolbox.Services;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    using Orleans.Runtime;

    public static class MARICoreServicesExtensions
    {
        /// <summary>
        /// Setups the core services if no previous implementation have been setups
        /// </summary>
        public static IServiceCollection SetupCoreServices(this IServiceCollection serviceCollection)
        {
            AddDefaultCoreService<ISequenceDefinitionProvider, SequenceDefinitionProvider>(serviceCollection);
            AddDefaultCoreService<IDefinitionProvider, DefinitionProvider>(serviceCollection);

            AddDefaultCoreService<IDiagnosticLogger, DiagnosticLogger>(serviceCollection);

            AddDefaultCoreService<IProcessSystemService, ProcessSystemService>(serviceCollection);

            serviceCollection.TryAddSingleton<IJsonSerializer>(NewtownJsonSerializer.Instance);
            AddDefaultCoreService<ITimeManager, TimeManager>(serviceCollection);

            serviceCollection.AddSingleton<IVGrainIdDedicatedFactory, VGrainIdFactoryDedicatedTemplates>();
            serviceCollection.AddSingleton<IVGrainIdDedicatedFactory, VGrainIdFactorySingleton>();
            AddDefaultCoreService<IVGrainIdDefaultFactory, VGrainIdFactoryDefault>(serviceCollection);

            AddDefaultCoreService<IVGrainIdFactory, VGrainIdFactory>(serviceCollection);
            AddDefaultCoreService<IVGrainProvider, VGrainProvider>(serviceCollection);

            AddDefaultCoreService<IMARIExecutionHandler, MARIExecutionHandler>(serviceCollection);

            AddDefaultCoreService<ISignalService, SignalService>(serviceCollection);
            AddDefaultCoreService<IWorkerTaskSchedulerProvider, WorkerTaskSchedulerProvider>(serviceCollection);

            AddDefaultCoreService<IRepositoryFactory, RepositoryFactory>(serviceCollection);
            AddDefaultCoreService<IAttributeToFactoryMapper<RepositoryAttribute>, RepositoryAttributeMapper>(serviceCollection);

            AddDefaultCoreService<IRemoteGrainServiceFactory, RemoteGrainServiceFactory>(serviceCollection);
            AddDefaultCoreService<IDynamicDefinitionHandler, DynamicDefinitionHandler>(serviceCollection);

            return serviceCollection;
        }

        /// <summary>
        /// Adds service <typeparamref name="TContract"/> using singleton implementation <typeparamref name="TImplementation"/>
        /// if it doesn't already exist
        /// </summary>
        private static void AddDefaultCoreService<TContract, TImplementation>(IServiceCollection serviceCollection, bool tryAdd = true)
            where TContract : class
            where TImplementation : class, TContract
        {
            //if (!serviceCollection.Any(s => s.ServiceType == typeof(TContract)))
            {
                if (tryAdd)
                    serviceCollection.TryAddSingleton<TContract, TImplementation>();
                else
                    serviceCollection.AddSingleton<TContract, TImplementation>();
            }
        }
    }
}
