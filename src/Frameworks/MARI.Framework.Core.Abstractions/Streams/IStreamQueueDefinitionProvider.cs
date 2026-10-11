// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Streams
{
    using MARI.Toolbox.Abstractions.Patterns.Strategy;

    using System;

    /// <summary>
    /// <see cref="StreamQueueDefinition"/> provider
    /// </summary>
    /// <seealso cref="IProviderStrategy{StreamQueueDefinition, Guid}" />
    public interface IStreamQueueDefinitionProvider : IProviderStrategy<StreamQueueDefinition, Guid>
    {
    }
}
