// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.Sequences
{
    using MARI.Framework.Core.Abstractions.Sequence;
    using MARI.Framework.Core.Abstractions.Sequence.Stages;

    using System.Linq.Expressions;

    /// <summary>
    /// Builder about <see cref="SequenceStagePushToContextDefinition"/> a stage able to push an information into <see cref="IExecutionContext"/>
    /// </summary>
    /// <seealso cref="ISequencePipelineStageDefinitionProvider" />
    internal sealed class SequencePipelinePushToContextStageBuilder<TInput> : SequencePipelineStageBaseBuilder, ISequencePipelineStageDefinitionProvider
    {
        #region Fields

        private readonly LambdaExpression _inputExpression;
        private readonly bool _override;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencePipelinePushToContextStageBuilder"/> class.
        /// </summary>
        public SequencePipelinePushToContextStageBuilder(LambdaExpression inputExpression,
                                                         bool @override,
                                                         Action<IDefinitionMetaDataWithDisplayNameBuilder>? metaDataBuilderAction,
                                                         Guid? fixUid)
            : base(metaDataBuilderAction, fixUid)
        {
            this._inputExpression = inputExpression;
            this._override = @override;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public SequenceStageDefinition ToDefinition()
        {
            var access = this._inputExpression.CreateAccess();

            var metaData = BuildDefinitionMetaData(out var displayName);

            return new SequenceStagePushToContextDefinition(this.FixUid,
                                                            typeof(TInput).GetAbstractType(),
                                                            displayName ?? "Push To Context",
                                                            access,
                                                            metaData,
                                                            this._override);
        }

        #endregion
    }
}
