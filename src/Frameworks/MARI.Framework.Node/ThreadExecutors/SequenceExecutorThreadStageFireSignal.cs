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
    using MARI.Framework.Core.Abstractions.Signals;
    using MARI.Framework.Node.Abstractions;
    using MARI.Framework.Node.Abstractions.Models;

    using MARI.Toolbox.Abstractions.Disposables;
    using MARI.Toolbox.Abstractions.Models;
    using MARI.Toolbox.Abstractions.Services;
    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Thread stage executor managing fire signal
    /// </summary>
    /// <seealso cref="ISequenceExecutorThreadStageHandler" />
    internal sealed class SequenceExecutorThreadStageFireSignal : SafeDisposable, ISequenceExecutorThreadStageHandler
    {
        #region Fields

        private readonly ISignalService _signalService;
        private readonly ITimeManager _timeManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorThreadStageCall"/> class.
        /// </summary>
        public SequenceExecutorThreadStageFireSignal(ISignalService signalService, ITimeManager timeManager)
        {
            this._signalService = signalService;
            this._timeManager = timeManager;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask<StageStepResult> ExecAsync(SequenceStageDefinition step,
                                                          object? input,
                                                          IExecutionContext sequenceContext,
                                                          ILogger logger,
                                                          IDiagnosticLogger diagnosticLogger,
                                                          IVGrainProvider vgrainProvider,
                                                          Func<ISecureContextToken<ISequenceExecutorThreadHandler>> securityThreadHandlerAccessor)
        {
            var fire = (SequenceStageFireSignalDefinition)step;

            var inputType = input is not null ? input.GetType() : (Type?)null;
            var inputTypeArg = inputType is not null ? TypedArgument.From(inputType) : null;

            SequenceExecutorThreadStageHandlerExtensions.StartStageWorking(this,
                                                                           diagnosticLogger,
                                                                           sequenceContext,
                                                                           this._timeManager,
                                                                           inputTypeArg);
            // FireAsync Signal

            object? data = fire.MessageAccess?.Resolve(input);

            if (fire.Multi && data is System.Collections.IEnumerable collection)
            {
                var sendTasks = collection.OfType<object>()
                                          .Select(d => FireAsync(sequenceContext, fire, d, input))
                                          .ToArray();

                await sendTasks.SafeWhenAllAsync(sequenceContext.CancellationToken);
            }
            else
            {
                await FireAsync(sequenceContext, fire, data, input);
            }

            // output diagnosticlog
            SequenceExecutorThreadStageHandlerExtensions.EndStageWorking(this,
                                                                         diagnosticLogger,
                                                                         sequenceContext,
                                                                         this._timeManager,
                                                                         inputTypeArg);

            return new StageStepResult(inputType, inputType is null ? Task.CompletedTask : input.GetTaskFrom(inputType));
        }

        /// <summary>
        /// Fires the specified signal with the specific message.
        /// </summary>
        private async Task FireAsync(IExecutionContext sequenceContext, SequenceStageFireSignalDefinition fire, object? data, object? input)
        {
            var signalInfo = fire.SignalInfo!.Resolve(input);

            Guid? signalId;
            string? signalName;

            signalId = signalInfo as Guid?;
            signalName = signalInfo as string;

            var info = signalInfo as SignalId?;
            if (info is not null)
            {
                signalId = info.Value.Uid;
                signalName = info.Value.Name;
            }

            if (signalId is not null && signalId != Guid.Empty)
            {
                await this._signalService.Fire(new SignalId(signalId.Value, signalName ?? string.Empty), data, sequenceContext.CancellationToken);
            }
            else if (!string.IsNullOrEmpty(signalName))
            {
                await this._signalService.Fire(signalName, data, sequenceContext.CancellationToken);
            }
            else
            {
                throw new SignalNotFoundException($"Stage '{fire.ToDebugDisplayName()}' -> No valid signal information id: '{signalId}'/'{signalName}'",
                                                  signalName ?? string.Empty,
                                                  MARIErrorCodes.Build(MARIErrorCodes.Categories.Signal, MARIErrorCodes.PartType.Execution, MARIErrorCodes.ErrorType.Invalid),
                                                  null);
            }
        }

        #endregion
    }
}
