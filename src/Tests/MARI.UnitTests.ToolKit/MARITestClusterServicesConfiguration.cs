// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.UnitTests.ToolKit
{
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Diagnostics;
    using MARI.Framework.Core.Abstractions.Repositories;
    using MARI.Framework.Core.Extensions;
    using MARI.Framework.Core.Repositories;
    using MARI.Framework.Node.Configurations;
    using MARI.Framework.Node.Extensions;
    using MARI.Framework.Node.Services;

    using MARI.Toolbox.Abstractions.Models;
    using MARI.Toolbox.Abstractions.Services;
    using MARI.Toolbox.Models;
    using MARI.Toolbox.Services;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using Orleans.Hosting;
    using Orleans.TestingHost;

    /// <summary>
    /// Service configuration use to 
    /// </summary>
    public sealed class MARITestClusterServicesConfiguration : ISiloConfigurator,
                                                                    IHostConfigurator,
                                                                    IClientBuilderConfigurator
    {
        #region Methods

        /// <inheritdoc />
        public void Configure(ISiloBuilder siloBuilder)
        {
            ConfigTestMARIServices(siloBuilder.Services);

            siloBuilder.AddMemoryGrainStorage(nameof(MARI))
                       .AddMemoryGrainStorageAsDefault()
                       .AddIncomingGrainCallFilter<IncomingGrainCallTracer>();
        }

        /// <inheritdoc />
        public void Configure(IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(ConfigTestMARIServices);
        }

        /// <inheritdoc />
        public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
        {
            clientBuilder.ConfigureServices(ConfigTestMARIServices);
        }

        /// <inheritdoc />
        private void ConfigTestMARIServices(IServiceCollection services)
        {
            MARICoreServicesExtensions.SetupCoreServices(services);

            services.AddSingleton<IVGrainProvider, VGrainProvider>()
                    .AddSingleton<IDiagnosticLogger, MARI.Framework.Core.Diagnostics.DiagnosticLogger>()
                    .AddSingleton<ITimeManager, TimeManager>()
                    .AddSingleton<IMARISerializer, MARISerializer>()
                    .AddSingleton<IObjectConverter, ObjectConverter>();

            services.SetupGrainRoutingServices();

            // Call
            services.SetupSequenceExecutorThreadStageProvider();

        }

        #endregion
    }
}
