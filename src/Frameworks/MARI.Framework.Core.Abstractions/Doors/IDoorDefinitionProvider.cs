// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Doors
{
    using MARI.Toolbox.Abstractions.Patterns.Strategy;

    using System;

    /// <summary>
    /// <see cref="DoorDefinition"/> provider
    /// </summary>
    /// <seealso cref="IProviderStrategy{DoorDefinition, Guid}" />
    public interface IDoorDefinitionProvider : IProviderStrategy<DoorDefinition, Guid>
    {
    }
}
