// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.ThreadExecutors
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Diagnostics;
    using MARI.Framework.Core.Abstractions.Sequence;
    using MARI.Framework.Core.Abstractions.Sequence.Stages;
    using MARI.Framework.Node.Abstractions;
    using MARI.Framework.Node.Abstractions.Models;
    using MARI.Toolbox.Abstractions.Disposables;
    using MARI.Toolbox.Abstractions.Models;
    using MARI.Toolbox.Abstractions.Services;
    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Dedicate filter typed about filter implementation
    /// </summary>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <seealso cref="ISequenceExecutorThreadStageHandler" />
    internal sealed class SequenceExecutorThreadStageFilter<TCollectionInput, TItemInput> : ISequenceExecutorThreadStageHandler
        where TCollectionInput : IEnumerable<TItemInput>
    {
        #region Fields

        private static readonly Type s_collectionTraits = typeof(TCollectionInput);
        private readonly ITimeManager _timeManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorThreadStageFilter{TCollectionInput, TItemInput}"/> class.
        /// </summary>
        public SequenceExecutorThreadStageFilter(ITimeManager timeManager)
        {
            this._timeManager = timeManager;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask<StageStepResult> ExecAsync(SequenceStageDefinition stepBase,
                                                    object? input,
                                                    IExecutionContext sequenceContext,
                                                    ILogger logger,
                                                    IDiagnosticLogger diagnosticLogger,
                                                    IVGrainProvider vgrainProvider,
                                                    Func<ISecureContextToken<ISequenceExecutorThreadHandler>> securityThreadHandlerAccessor)
        {
            var step = (SequenceStageFilterDefinition)stepBase;

            var collection = (TCollectionInput?)input;

            if (collection is null || !collection.Any())
                return ValueTask.FromResult(new StageStepResult(s_collectionTraits, EnumerableHelper<TItemInput>.ReadOnlyArray.GetTaskFrom(s_collectionTraits)));

            // input diagnosticlog
            SequenceExecutorThreadStageHandlerExtensions.StartStageWorking(this,
                                                                           diagnosticLogger,
                                                                           sequenceContext,
                                                                           this._timeManager,
                                                                           TypedArgument.From(input, s_collectionTraits));

            // Filter

            var expressionFilter = step.Condition.ToExpression<TItemInput, bool>();
            var expressionFilterFunc = expressionFilter.Compile();

            var filteredResults = collection?.Where(expressionFilterFunc).ToArray();

            // output diagnosticlog
            SequenceExecutorThreadStageHandlerExtensions.EndStageWorking(this,
                                                                         diagnosticLogger,
                                                                         sequenceContext,
                                                                         this._timeManager,
                                                                         TypedArgument.From(filteredResults, s_collectionTraits));

            return ValueTask.FromResult(new StageStepResult(s_collectionTraits, filteredResults.GetTaskFrom(s_collectionTraits)));
        }

        #endregion
    }
}
