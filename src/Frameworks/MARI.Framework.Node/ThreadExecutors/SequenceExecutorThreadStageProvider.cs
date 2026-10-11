// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.ThreadExecutors
{
    using MARI.Framework.Core.Abstractions.Exceptions;
    using MARI.Framework.Core.Abstractions.Sequence;
    using MARI.Framework.Node.Abstractions;
    using MARI.Toolbox.Helpers;

    using System.Collections.Generic;
    using System.Linq;

    internal sealed class SequenceExecutorThreadStageProvider : ISequenceExecutorThreadStageProvider
    {
        #region Fields
        
        private readonly IReadOnlyCollection<ISequenceExecutorThreadStageSourceProvider> _sourceProviders;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorThreadStageProvider"/> class.
        /// </summary>
        public SequenceExecutorThreadStageProvider(IEnumerable<ISequenceExecutorThreadStageSourceProvider> sourceProviders)
        {
            this._sourceProviders = sourceProviders?.ToArray() ?? EnumerableHelper<ISequenceExecutorThreadStageSourceProvider>.ReadOnlyArray;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool CanHandler(SequenceStageDefinition stage)
        {
            return this._sourceProviders.Any(s => s.CanHandler(stage));
        }

        /// <inheritdoc />
        public ISequenceExecutorThreadStageHandler Provide(SequenceStageDefinition stage)
        {
            var sourceProvider = this._sourceProviders.FirstOrDefault(s => s.CanHandler(stage));

            if (sourceProvider is null)
                throw new StageExecutorNotFoundException(stage);

            return sourceProvider.Provide(stage);
        }

        #endregion
    }
}
