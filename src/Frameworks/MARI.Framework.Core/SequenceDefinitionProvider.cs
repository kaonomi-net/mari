// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Sequence;

    using Microsoft.Extensions.Logging;

    using System.Collections.Generic;

    /// <inheritdoc cref="ISequenceDefinitionProvider"/>
    /// <seealso cref="ISequenceDefinitionProvider" />
    internal sealed class SequenceDefinitionProvider : DefinitionBaseProvider<SequenceDefinition, ISequenceDefinitionSourceProvider>, ISequenceDefinitionProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceDefinitionProvider"/> class.
        /// </summary>
        public SequenceDefinitionProvider(IEnumerable<ISequenceDefinitionSourceProvider> specificDefinitionProviderSources,
                                          IEnumerable<IDefinitionSourceProvider<SequenceDefinition>> genericDefinitionSourceProviders,
                                          ILogger<ISequenceDefinitionSourceProvider> logger) 
            : base(specificDefinitionProviderSources, genericDefinitionSourceProviders, logger)
        {
        }

        #endregion
    }
}
