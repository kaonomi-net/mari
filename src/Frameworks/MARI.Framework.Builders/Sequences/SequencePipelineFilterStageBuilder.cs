// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.Sequences
{
    using MARI.Framework.Core.Abstractions.Sequence;
    using MARI.Framework.Core.Abstractions.Sequence.Stages;
    using MARI.Toolbox.Abstractions.Conditions;
    using MARI.Toolbox.Models;

    using System;

    /// <summary>
    /// Stage about loop throught <typeparamref name="TInput"/> to apply a filter
    /// </summary>
    /// <seealso cref="ISequencePipelineInternalStage" />
    internal sealed class SequencePipelineFilterStageBuilder<TInput> : SequencePipelineStageBaseBuilder, ISequencePipelineStageDefinitionProvider
    {
        #region Fields

        private readonly ConditionExpressionDefinition _condition;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelineForeachBuilder"/> class.
        /// </summary>
        public SequencePipelineFilterStageBuilder(ConditionExpressionDefinition condition,
                                                  Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilderAction,
                                                  Guid? fixUid)
            : base(metaDataBuilderAction, fixUid)
        {
            this._condition = condition;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public SequenceStageDefinition ToDefinition()
        {
            var metadata = BuildDefinitionMetaData(out var displayName);

            return new SequenceStageFilterDefinition(this.FixUid,
                                                     (CollectionType)typeof(TInput).GetAbstractType(),
                                                     displayName ?? "Filter",
                                                     this._condition,
                                                     metadata);
        }

        #endregion
    }
}
