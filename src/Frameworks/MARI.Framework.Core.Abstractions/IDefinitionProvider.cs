// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions
{
    using MARI.Toolbox.Abstractions.Patterns.Strategy;

    using System;

    /// <summary>
    /// Provider used to get access to all definitions
    /// </summary>
    public interface IDefinitionProvider : IProviderStrategy<IDefinition, Guid>
    {
    }

    /// <summary>
    /// Provider used to get access to all definitions about a specific type a used as source for <see cref="IDefinitionProvider"/>
    /// </summary>
    public interface ISpecificDefinitionSourceProvider : IProviderStrategySource<IDefinition, Guid>
    {
    }
}
