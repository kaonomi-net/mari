// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.ThreadExecutors
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Diagnostics;
    using MARI.Framework.Core.Abstractions.Exceptions;
    using MARI.Framework.Core.Abstractions.Sequence;
    using MARI.Framework.Core.Abstractions.Sequence.Stages;
    using MARI.Framework.Core.Diagnostics;
    using MARI.Framework.Node.Abstractions;
    using MARI.Framework.Node.Abstractions.Models;
    using MARI.Toolbox;
    using MARI.Toolbox.Abstractions.Disposables;
    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Thread stage executor managing foreach call
    /// </summary>
    /// <seealso cref="ISequenceExecutorThreadStageSourceProvider" />
    /// <seealso cref="ISequenceExecutorThreadStageHandler" />
    internal sealed class SequenceExecutorThreadStageForeach : ISequenceExecutorThreadStageHandler
    {
        #region Fields

        private const string ERROR_NOT_ALLOW_INNER = "[Technical] MUST not execute foreach if already exist some inner threads ({IExecutionContext})";

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
            var step = (SequenceStageForeachDefinition)stepBase;
            ArgumentNullException.ThrowIfNull(step);

            if (step.MemberAccess is not null)
                input = step.MemberAccess.Resolve(input);

            using (var token = securityThreadHandlerAccessor())
            {
                // set post process to aggregate result
                token.Token.RegisterPostProcess((def, accessor) => ForeachSequenceStagePostProcess(def, accessor, logger));

                // Get inner thread by input element
                if (!token.Token.HasInnerThreads && input is System.Collections.IEnumerable collection)
                {
                    var innerThreads = collection.Cast<object>()
                                                 .Select(subInput => token.Token.CreateInnerThread(new SequenceExecutorExecThreadState(sequenceContext.FlowUID,
                                                                                                                                       step.InnerFlow.Uid,
                                                                                                                                       Guid.NewGuid(),
                                                                                                                                       sequenceContext.CurrentExecutionId,
                                                                                                                                       step.InnerFlow,
                                                                                                                                       subInput),
                                                                                                    step.InnerFlow,
                                                                                                    sequenceContext))
                                                 .ToArray();

                    foreach (var inner in innerThreads)
                        diagnosticLogger.Log(ExecutionContextChangeDiagnosticLog.From(inner.ExecutionContext));

                    var currentState = token.Token.GetCurrentDoneThreadState();

                    // Register inner execution states
                    token.Token.SetInnerThreads(innerThreads);

                    var result = new StageStepResult(null, Task.CompletedTask);
                    return ValueTask.FromResult(result);
                }

                if (token.Token.HasInnerThreads)
                {
                    logger.OptiLog(LogLevel.Error, ERROR_NOT_ALLOW_INNER, sequenceContext);
                    throw new VGrainInvalidOperationMARIException(ERROR_NOT_ALLOW_INNER, sequenceContext);
                }
            }
            throw new InvalidOperationException();
        }

        #region Tools

        /// <summary>
        /// Call after a <see cref="ForeachSequenceStageStepDefinition"/> stage to aggregate results
        /// </summary>
        /// <remarks>
        ///     All post process are execute
        /// </remarks>
        private Task<StageStepResult> ForeachSequenceStagePostProcess(SequenceStageDefinition sequenceStageDefinition,
                                                                      Func<ISecureContextToken<ISequenceExecutorThreadHandler>> securityThreadHandlerAccessor,
                                                                      ILogger logger)
        {
            using (var handlerSafeToken = securityThreadHandlerAccessor())
            {
                var collectionType = NoneType.Trait;
                Task task = Task.FromResult(NoneType.Instance);

                if (handlerSafeToken.Token.HasInnerThreads && handlerSafeToken.Token.AllInnerThreadsJobDone)
                {
                    var foreachThreads = handlerSafeToken.Token.PullInnerThreads(true);
                    var foreachThreadsHandler = foreachThreads.Select(f => f.GetSecurityThreadHandler()).ToArray();

                    try
                    {
                        var foreachIndexedState = foreachThreadsHandler.ToDictionary(k => k, kv => kv.Token.GetCurrentDoneThreadState());

                        //TO-DO from Opus 5.5 Partial foreach failures silently dropped : An error is raised only when every inner thread failed. Otherwise the failed items are filtered out of the results and the stage reports success with missing elements. Raise or report an error when any inner thread failed, or make the behaviour an explicit option.
                        if (foreachIndexedState.All(i => i.Value.Exception != null))
                        {
                            var aggreException = foreachIndexedState.Where(f => f.Value?.Exception != null)
                                                                    .Select(e => e.Value.Exception!)
                                                                    .ToArray();

                            throw new AggregateException(aggreException);
                        }

                        // Partial failures : failed items are excluded from the result, at least report them
                        var partialFailures = foreachIndexedState.Where(f => f.Value?.Exception != null)
                                                                 .Select(e => e.Value.Exception!)
                                                                 .ToArray();

                        if (partialFailures.Length > 0)
                        {
                            logger.OptiLog(LogLevel.Error,
                                           "Foreach stage {stageUid} : {failedCount}/{totalCount} inner executions failed, their results are excluded : {exception}",
                                           sequenceStageDefinition?.Uid,
                                           partialFailures.Length,
                                           foreachIndexedState.Count,
                                           new AggregateException(partialFailures));
                        }

                        ArgumentNullException.ThrowIfNull(sequenceStageDefinition);
                        var step = (SequenceStageForeachDefinition)sequenceStageDefinition;

                        collectionType = step.OutputForeach?.ToType().MakeArrayType();

                        if (step.OutputForeach is not null && collectionType != null)
                        {
                            object? result = Activator.CreateInstance(collectionType, new object[] { 0 });

                            if (foreachThreadsHandler.Length > 0)
                            {
                                // Store thread results
                                var objResult = foreachThreadsHandler.Select(t => foreachIndexedState[t].Output)
                                                                     .Where(t => t != null && t is NoneType == false)
                                                                     .ToArray();

                                var resultArray = (Array?)Activator.CreateInstance(collectionType, new object[] { objResult.Length });

                                if (resultArray != null)
                                    Array.Copy(objResult, resultArray, objResult.Length);

                                result = resultArray;
                            }

                            if (sequenceStageDefinition is SequenceStageForeachDefinition foreachDef && foreachDef.SetMethod is not null)
                            {
                                var state = handlerSafeToken.Token.GetCurrentDoneThreadState();
                                var collection = result;
                                var sourceInst = state.Output;

                                if (sourceInst is not null)
                                {
                                    var mtdh = foreachDef.SetMethod.ToMethod(sourceInst.GetType());

                                    if (mtdh is null)
                                        throw new ArgumentNullException($"Foreach call couldn't found set method {foreachDef.SetMethod} in type {sourceInst.GetType()}");

                                    mtdh.Invoke(sourceInst, new [] { collection });

                                    // Define output type
                                    collectionType = sourceInst.GetType();
                                }

                                result = sourceInst;
                            }

                            task = result.GetTaskFrom(collectionType);
                        }
                    }
                    finally
                    {
                        foreach (var handler in foreachThreadsHandler)
                            handler.Dispose();
                    }
                }

                return Task.FromResult(new StageStepResult(collectionType, task));
            }
        }

        #endregion

        #endregion
    }
}
