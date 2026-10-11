// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Signals
{
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions.Signals;
    using MARI.Toolbox.Patterns.Strategy;

    using System;

    /// <summary>
    /// In memory provider source of <see cref="SignalDefinition"/>
    /// </summary>
    /// <seealso cref="ProviderStrategyBaseSource{SignalDefinition, Guid}" />
    /// <seealso cref="ISignalDefinitionProviderSource" />
    public sealed class InMemorySignalDefinitionProviderSource : InMemoryBaseDefinitionProvider<SignalDefinition>, ISignalDefinitionProviderSource
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemorySignalDefinitionProviderSource"/> class.
        /// </summary>
        public InMemorySignalDefinitionProviderSource(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        #endregion
    }
}
