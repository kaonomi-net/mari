// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Triggers
{
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Triggers;

    using MARI.Toolbox.Patterns.Strategy;

    using System;

    /// <summary>
    /// In memory provider source of <see cref="ITriggerDefinition"/>
    /// </summary>
    /// <seealso cref="ProviderStrategyBaseSource{TriggerDefinition, Guid}" />
    /// <seealso cref="IStreamQueueDefinitionProviderSource" />
    public sealed class InMemoryTriggerDefinitionProviderSource : InMemoryBaseDefinitionProvider<TriggerDefinition>, ITriggerDefinitionProviderSource, IDefinitionInMemoryFillSourceProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryTriggerDefinitionProviderSource"/> class.
        /// </summary>
        public InMemoryTriggerDefinitionProviderSource(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        #endregion
    }
}
