// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.Sequences
{
    using MARI.Framework.Builders;
    using MARI.Framework.Core.Abstractions.Sequence;

    /// <summary>
    /// Sequence builder
    /// </summary>
    public interface ISequenceBuilder : ISequenceTriggerBuilder, IDefinitionBaseBuilder<SequenceDefinition>
    {
        #region Properties

        /// <summary>
        /// Gets the parent builder.
        /// </summary>
        ISequenceBuilder? Parent { get; }

        #endregion

    }
}
