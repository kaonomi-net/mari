// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions
{
    using MARI.Framework.Core.Abstractions.Sequence;
    using MARI.Toolbox.Abstractions.Patterns.Strategy;

    /// <summary>
    /// Provider used to get <see cref="SequenceDefinition"/> from <see cref="ISequenceDefinitionSourceProvider"/>
    /// </summary>
    /// <seealso cref="IProviderStrategy{SequenceDefinition, Guid}" />
    public interface ISequenceDefinitionProvider : IProviderStrategy<SequenceDefinition, Guid>
    {
    }
}
