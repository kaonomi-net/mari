// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions
{
    using MARI.Framework.Core.Abstractions.Sequence;

    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Module in charge to provide <see cref="IReadOnlyCollection{SequenceDefinition}"/>
    /// </summary>
    public interface ISequenceModule
    {
        /// <summary>
        /// Gets the sequence definitions.
        /// </summary>
        ValueTask<IReadOnlyCollection<SequenceDefinition>> GetSequenceDefinitionsAsync();
    }
}
