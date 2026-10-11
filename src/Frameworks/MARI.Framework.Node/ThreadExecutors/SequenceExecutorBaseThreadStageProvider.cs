// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.ThreadExecutors
{
    using MARI.Framework.Core.Abstractions.Sequence;
    using MARI.Framework.Node.Abstractions;

    using MARI.Toolbox.Disposables;

    /// <summary>
    /// Base Thread stage executor
    /// </summary>
    /// <seealso cref="ISequenceExecutorThreadStageSourceProvider" />
    /// <seealso cref="ISequenceExecutorThreadStageHandler" />
    internal abstract class SequenceExecutorBaseThreadStageProvider<TStageDefinition> : SafeDisposable, ISequenceExecutorThreadStageSourceProvider
        where TStageDefinition : SequenceStageDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorBaseThreadStageProvider"/> class.
        /// </summary>
        public SequenceExecutorBaseThreadStageProvider()
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual bool CanHandler(SequenceStageDefinition? stage)
        {
            return stage is TStageDefinition;
        }

        /// <inheritdoc />
        public ISequenceExecutorThreadStageHandler Provide(SequenceStageDefinition? stageBase)
        {
            return OnProvide((TStageDefinition?)stageBase);
        }

        /// <inheritdoc cref="ISequenceExecutorThreadStageSourceProvider.Provide(SequenceStageDefinition)" />
        protected abstract ISequenceExecutorThreadStageHandler OnProvide(TStageDefinition? stageBase);

        #endregion
    }
}
