// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions
{
    using MARI.Toolbox.Abstractions.Patterns.Strategy;

    using System;

    /// <summary>
    /// Provider used to provide any definition
    /// </summary>
    public interface IDefinitionSourceProvider<TDefinition> : IProviderStrategySource<TDefinition, Guid>
        where TDefinition : class, IDefinition

    {
    }
}
