// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.Sequences
{
    using MARI.Framework.Core.Abstractions.Sequence;

    /// <summary>
    /// Get pipeline stage
    /// </summary>
    public interface ISequencePipelineStageDefinitionProvider
    {
        /// <summary>
        /// Converts to definition.
        /// </summary>
        SequenceStageDefinition ToDefinition();
    }
}
