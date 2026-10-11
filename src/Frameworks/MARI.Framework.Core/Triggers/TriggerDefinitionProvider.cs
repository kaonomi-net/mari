// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Triggers
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Triggers;

    using Microsoft.Extensions.Logging;

    using System.Collections.Generic;

    /// <summary>
    /// Provider in charge to give access to <see cref="TriggerDefinition"/>
    /// </summary>
    /// <seealso cref="ITriggerDefinitionProvider" />
    public sealed class TriggerDefinitionProvider : DefinitionBaseProvider<TriggerDefinition, ITriggerDefinitionProviderSource>, ITriggerDefinitionProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerDefinitionProvider"/> class.
        /// </summary>
        public TriggerDefinitionProvider(IEnumerable<ITriggerDefinitionProviderSource> specificDefinitionProviderSources,
                                         IEnumerable<IDefinitionSourceProvider<TriggerDefinition>> genericDefinitionSourceProviders,
                                         ILogger<ITriggerDefinitionProviderSource> logger) 
            : base(specificDefinitionProviderSources, genericDefinitionSourceProviders, logger)
        {
        }

        #endregion
    }
}
