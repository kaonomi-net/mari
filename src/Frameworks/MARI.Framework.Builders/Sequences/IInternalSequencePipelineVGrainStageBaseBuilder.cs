// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.Sequences
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Sequence;

    /// <summary>
    /// 
    /// </summary>
    internal interface IInternalSequencePipelineVGrainStageBaseBuilder
    {
        /// <summary>
        /// Gets the sequence pipeline builder.
        /// </summary>
        ISequencePipelineBaseBuilder SequencePipelineBuilder { get; }

        /// <summary>
        /// Prevents the output.
        /// </summary>
        void PreventOutput();

        /// <summary>
        /// Converts to definition.
        /// </summary>
        SequenceStageDefinition? ToDefinition();

        /// <summary>
        /// Builds the definition meta data.
        /// </summary>
        DefinitionMetaData? BuildDefinitionMetaData(out string? displayName);
    }
}
