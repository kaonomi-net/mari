// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Streams
{
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions.Streams;

    using MARI.Toolbox.Patterns.Strategy;

    using System;

    /// <summary>
    /// Provider stat store in memory the definition
    /// </summary>
    /// <seealso cref="ProviderStrategyBaseSource{StreamQueueDefinition, System.Guid}" />
    /// <seealso cref="IStreamQueueDefinitionProviderSource" />
    public sealed class InMemoryStreamQueueDefinitionProviderSource : InMemoryBaseDefinitionProvider<StreamQueueDefinition>, IStreamQueueDefinitionProviderSource
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryStreamQueueDefinitionProviderSource"/> class.
        /// </summary>
        public InMemoryStreamQueueDefinitionProviderSource(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        #endregion
    }
}
