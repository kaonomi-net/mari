// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Signals
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Doors;

    using Microsoft.Extensions.Logging;

    using System.Collections.Generic;

    /// <summary>
    /// Provider in charge to give access to <see cref="DoorDefinition"/>
    /// </summary>
    /// <seealso cref="IDoorDefinitionProvider" />
    internal sealed class DoorDefinitionProvider : DefinitionBaseProvider<DoorDefinition, IDoorDefinitionProviderSource>, IDoorDefinitionProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorDefinitionProvider"/> class.
        /// </summary>
        public DoorDefinitionProvider(IEnumerable<IDoorDefinitionProviderSource> specificDefinitionProviderSources,
                                      IEnumerable<IDefinitionSourceProvider<DoorDefinition>> genericDefinitionSourceProviders,
                                      ILogger<IDoorDefinitionProviderSource> logger) 
            : base(specificDefinitionProviderSources, genericDefinitionSourceProviders, logger)
        {
        }

        #endregion

    }
}
