// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Signals
{
    using MARI.Toolbox.Abstractions.Patterns.Strategy;

    using System;

    /// <summary>
    /// <see cref="SignalDefinition"/> provider
    /// </summary>
    /// <seealso cref="IProviderStrategy{SignalDefinition, Guid}" />
    public interface ISignalDefinitionProvider : IProviderStrategy<SignalDefinition, Guid>
    {
    }
}
