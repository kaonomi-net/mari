// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.ThreadExecutors
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Customizations;
    using MARI.Framework.Core.Abstractions.Diagnostics;
    using MARI.Framework.Core.Abstractions.Exceptions;
    using MARI.Framework.Core.Abstractions.Repositories;
    using MARI.Framework.Core.Abstractions.Sequence;
    using MARI.Framework.Core.Abstractions.Sequence.Stages;
    using MARI.Framework.Node.Abstractions;
    using MARI.Framework.Node.Abstractions.Models;

    using MARI.Toolbox.Abstractions.Disposables;
    using MARI.Toolbox.Abstractions.Services;
    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Thread stage executor managing inner sequence call
    /// </summary>
    /// <seealso cref="ISequenceExecutorThreadStageSourceProvider" />
    /// <seealso cref="ISequenceExecutorThreadStageHandler" />
    internal sealed class SequenceExecutorThreadStageNestedSequenceCall : SafeDisposable, ISequenceExecutorThreadStageHandler
    {
        #region Fields

        private readonly IMARIExecutionHandler _mariExecutionHandler;
        private readonly IMARISerializer _mariSerializer;
        private readonly ITimeManager _timeManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorThreadStageNestedSequenceCall"/> class.
        /// </summary>
        public SequenceExecutorThreadStageNestedSequenceCall(ITimeManager timeManager,
                                                             IMARIExecutionHandler mariExecutionHandler,
                                                             IMARISerializer mariSerializer)
        {
            this._mariExecutionHandler = mariExecutionHandler;
            this._mariSerializer = mariSerializer;
            this._timeManager = timeManager;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask<StageStepResult> ExecAsync(SequenceStageDefinition step,
                                                    object? input,
                                                    IExecutionContext sequenceContext,
                                                    ILogger logger,
                                                    IDiagnosticLogger diagnosticLogger,
                                                    IVGrainProvider vgrainProvider,
                                                    Func<ISecureContextToken<ISequenceExecutorThreadHandler>> securityThreadHandlerAccessor)
        {
            var nestedCall = (SequenceStageNestedSequenceCallDefinition)step;

            var sequenceInput = nestedCall.SequenceInput?.Resolve(input);
            var outpoutType = nestedCall.RelayInput == false && nestedCall.Output is not null ? nestedCall.Output!.ToType() : null;

            using (var token = securityThreadHandlerAccessor())
            {
                // set post process to aggregate result
                token.Token.RegisterPostProcess(SequenceStageNestedPostProcess);

                var customization = nestedCall.CustomizationDescriptions;
                var executionCustomization = token.Token.GetSequenceExecutionCustomization();

                customization = ExecutionCustomizationDescriptionsExtensions.Merge(executionCustomization,
                                                                                   customization,
                                                                                   d => d.StageUid == step.Uid ? Guid.Empty : d.StageUid);

                Task resultTask = this._mariExecutionHandler.SequenceWithInput(nestedCall.SequenceId, customization)
                                                                 .SetInput(sequenceInput)
                                                                 .From(sequenceContext, true)
                                                                 .RunWithAnyResultAsync(sequenceContext.CancellationToken);

                ArgumentNullException.ThrowIfNull(resultTask);

                return ValueTask.FromResult(new StageStepResult(typeof(IExecutionResult), resultTask));
            }
        }

        #region Tools

        private Task<StageStepResult> SequenceStageNestedPostProcess(SequenceStageDefinition sequenceStageDefinition,
                                                                     Func<ISecureContextToken<ISequenceExecutorThreadHandler>> securityThreadHandlerAccessor)
        {
            using (var handlerSafeToken = securityThreadHandlerAccessor())
            {
                var nestedCall = (SequenceStageNestedSequenceCallDefinition)sequenceStageDefinition;

                var thread = handlerSafeToken.Token.GetCurrentInProcessThreadState();

                if (thread is null)
                    throw new SequenceExecutionException("Post process doesn't have any call thread");

                if (thread.Exception is not null || !string.IsNullOrEmpty(thread.ErrorMessage))
                    throw new SequenceExecutionException($"[Flow Id: {thread.FlowUid}] Post process calling nested sequence stage {nestedCall.Uid} -> sequence {nestedCall.SequenceId} : {thread.ErrorMessage}", thread.Exception);

                var originInput = thread.ThreadInput;
                var sequenceCallOutput = thread.Output as IExecutionResult;

                ArgumentNullException.ThrowIfNull(sequenceCallOutput);

                if (sequenceCallOutput.Cancelled)
                    throw new OperationCanceledException();

                if (!string.IsNullOrEmpty(sequenceCallOutput.ErrorCode))
                    throw new SequenceExecutionException($"[Flow Id: {thread.FlowUid}] Post process calling nested sequence stage {nestedCall.Uid} -> sequence {nestedCall.SequenceId} ErrorCode : {sequenceCallOutput.ErrorCode}", thread.Exception);

                // Output

                object? output = null;

                if (sequenceCallOutput.HasOutput)
                    output = sequenceCallOutput.GetOutput();

                if (originInput is not null && nestedCall.SetMethod is not null)
                {
                    var mth = nestedCall.SetMethod.ToMethod(originInput.GetType());

                    if (mth is null)
                        throw new ArgumentNullException($"[Flow Id: {thread.FlowUid}] Post process calling nested sequence stage {nestedCall.Uid} -> sequence {nestedCall.SequenceId} ErrorCode : SetMethod not found {nestedCall.SetMethod} in type {originInput.GetType()}");

                    mth.Invoke(originInput, new[] { output });
                }

                if (nestedCall.PreventReturn)
                    return Task.FromResult(new StageStepResult(null, Task.CompletedTask));

                if (nestedCall.RelayInput)
                    return Task.FromResult(new StageStepResult(originInput?.GetType(), originInput?.GetTaskFrom() ?? Task.CompletedTask));

                return Task.FromResult(new StageStepResult(output?.GetType(), output?.GetTaskFrom() ?? Task.CompletedTask));
            }
        }

        #endregion

        #endregion
    }
}
