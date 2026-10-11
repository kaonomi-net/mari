// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.UnitTests.ToolKit.Services
{
    using MARI.Framework.Node.Configurations;
    using MARI.Framework.Node.ThreadExecutors;

    using Microsoft.Extensions.DependencyInjection;

    public static class SequenceExecutorThreadStageProviderMock
    {
        public static object Create(IServiceCollection? services = null)
        {
            services ??= new ServiceCollection();
            services.SetupSequenceExecutorThreadStageProvider();

            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<ISequenceExecutorThreadStageProvider>();
        }
    }
}