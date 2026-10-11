// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Streams
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Streams;
    using MARI.Toolbox.Patterns.Strategy;

    using Microsoft.Extensions.Logging;

    using System.Collections.Generic;

    /// <summary>
    /// Use all <see cref="IStreamQueueDefinitionProviderSource"/> to found the corresponding <see cref="StreamQueueDefinition"/>
    /// </summary>
    /// <seealso cref="ProviderStrategyBase{StreamQueueDefinition, System.Guid, MARI.Framework.Core.Abstractions.Streams.IStreamQueueDefinitionProviderSource}" />
    /// <seealso cref="IStreamQueueDefinitionProvider" />
    internal sealed class StreamQueueDefinitionProvider : DefinitionBaseProvider<StreamQueueDefinition, IStreamQueueDefinitionProviderSource>, IStreamQueueDefinitionProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamQueueDefinitionProvider"/> class.
        /// </summary>
        public StreamQueueDefinitionProvider(IEnumerable<IStreamQueueDefinitionProviderSource> specificDefinitionProviderSources,
                                             IEnumerable<IDefinitionSourceProvider<StreamQueueDefinition>> genericDefinitionSourceProviders,
                                             ILogger<IStreamQueueDefinitionProviderSource> logger) 
            : base(specificDefinitionProviderSources, genericDefinitionSourceProviders, logger)
        {
        }

        #endregion
    }
}
