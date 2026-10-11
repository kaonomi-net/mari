// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions
{
    using MARI.Framework.Core.Abstractions.Sequence;

    /// <summary>
    /// Provider used to access <see cref="SequenceDefinition"/>
    /// </summary>
    /// <remarks>
    ///     Use <see cref="ISequenceDefinitionProvider"/> to access <see cref="SequenceDefinition"/>
    /// </remarks>
    public interface ISequenceDefinitionSourceProvider : IDefinitionSourceProvider<SequenceDefinition>
    {
    }
}
