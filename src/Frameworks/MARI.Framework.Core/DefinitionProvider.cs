// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core
{
    using MARI.Framework.Core.Abstractions;

    using MARI.Toolbox.Patterns.Strategy;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Single entry point to access all definitions
    /// </summary>
    /// <seealso cref="MARI.Toolbox.Patterns.Strategy.ProviderStrategyBase{IDefinition, System.Guid, MARI.Framework.Core.Abstractions.ISpecificDefinitionSourceProvider}" />
    /// <seealso cref="MARI.Framework.Core.Abstractions.IDefinitionProvider" />
    internal sealed class DefinitionProvider : ProviderStrategyBase<IDefinition, Guid, ISpecificDefinitionSourceProvider>, IDefinitionProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionProvider"/> class.
        /// </summary>
        public DefinitionProvider(IEnumerable<ISpecificDefinitionSourceProvider> providerSource, ILogger<IDefinitionProvider>? logger) 
                : base(providerSource.Distinct(), logger)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override Expression<Func<IDefinition, bool>> GetFetchByKeyExpressions(IReadOnlyCollection<Guid> keys)
        {
            return d => keys.Contains(d.Uid);
        }

        #endregion
    }
}
