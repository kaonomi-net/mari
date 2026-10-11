// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Triggers
{
    using MARI.Toolbox.Abstractions.Patterns.Strategy;

    using System;

    /// <summary>
    /// <see cref="TriggerDefinition"/> provider
    /// </summary>
    /// <seealso cref="IProviderStrategy{ITriggerDefinition, Guid}" />
    public interface ITriggerDefinitionProvider : IProviderStrategy<TriggerDefinition, Guid>
    {
    }
}
