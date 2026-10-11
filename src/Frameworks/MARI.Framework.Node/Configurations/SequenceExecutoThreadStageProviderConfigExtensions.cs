// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Configurations
{
    using MARI.Framework.Core.Abstractions.Sequence.Stages;
    using MARI.Framework.Node.Abstractions;
    using MARI.Framework.Node.ThreadExecutors;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    internal static class SequenceExecutoThreadStageProviderConfigExtensions
    {
        public static void SetupSequenceExecutorThreadStageProvider(this IServiceCollection serviceCollection)
        {
            // Thread Stage Executor
            serviceCollection.TryAddSingleton<ISequenceExecutorThreadStageProvider, SequenceExecutorThreadStageProvider>();

            // Push to context
            serviceCollection.AddSingleton<SequenceExecutorThreadStagePushToContext>();
            serviceCollection.AddSingleton<ISequenceExecutorThreadStageSourceProvider, SequenceExecutorGenericThreadStageSourceProvider<SequenceStagePushToContextDefinition, SequenceExecutorThreadStagePushToContext>>();

            // Foreach
            serviceCollection.AddSingleton<SequenceExecutorThreadStageForeach>();
            serviceCollection.AddSingleton<ISequenceExecutorThreadStageSourceProvider, SequenceExecutorGenericThreadStageSourceProvider<SequenceStageForeachDefinition, SequenceExecutorThreadStageForeach>>();

            // Call
            serviceCollection.AddSingleton<SequenceExecutorThreadStageCall>();
            serviceCollection.AddSingleton<ISequenceExecutorThreadStageSourceProvider, SequenceExecutorGenericThreadStageSourceProvider<SequenceStageCallDefinition, SequenceExecutorThreadStageCall>>();

            // Filter
            serviceCollection.AddSingleton<ISequenceExecutorThreadStageSourceProvider, SequenceExecutorFilterThreadStageProvider>();

            // FireAsync Signal
            serviceCollection.AddSingleton<SequenceExecutorThreadStageFireSignal>();
            serviceCollection.AddSingleton<ISequenceExecutorThreadStageSourceProvider, SequenceExecutorGenericThreadStageSourceProvider<SequenceStageFireSignalDefinition, SequenceExecutorThreadStageFireSignal>>();

            // Select
            serviceCollection.AddSingleton<SequenceExecutorThreadStageSelect>();
            serviceCollection.AddSingleton<ISequenceExecutorThreadStageSourceProvider, SequenceExecutorGenericThreadStageSourceProvider<SequenceStageSelectDefinition, SequenceExecutorThreadStageSelect>>();

            // Nested Sequence call
            serviceCollection.AddSingleton<SequenceExecutorThreadStageNestedSequenceCall>();
            serviceCollection.AddSingleton<ISequenceExecutorThreadStageSourceProvider, SequenceExecutorGenericThreadStageSourceProvider<SequenceStageNestedSequenceCallDefinition, SequenceExecutorThreadStageNestedSequenceCall>>();

        }
    }
}
