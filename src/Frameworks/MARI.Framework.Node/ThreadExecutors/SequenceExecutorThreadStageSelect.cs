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
    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Thread stage executor managing select
    /// </summary>
    /// <seealso cref="ISequenceExecutorThreadStageHandler" />
    internal sealed class SequenceExecutorThreadStageSelect : SafeDisposable, ISequenceExecutorThreadStageHandler
    {
        #region Fields
        
        private readonly ITimeManager _timeManager;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceExecutorThreadStageSelect"/> class.
        /// </summary>
        public SequenceExecutorThreadStageSelect(ITimeManager timeManager)
        {
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
            var select = (SequenceStageSelectDefinition)step;

            var inputType = input is not null ? input.GetType() : (Type?)null;
            var inputTypeArg = inputType is not null ? TypedArgument.From(inputType) : null;

            SequenceExecutorThreadStageHandlerExtensions.StartStageWorking(this,
                                                                           diagnosticLogger,
                                                                           sequenceContext,
                                                                           this._timeManager,
                                                                           inputTypeArg);

            var output = select.SelectAccess.Resolve(input);

            var outputType = output is not null ? output.GetType() : select.Output?.ToType();
            var outputTypeArg = outputType is not null ? TypedArgument.From(outputType) : null;

            // output diagnosticlog
            SequenceExecutorThreadStageHandlerExtensions.EndStageWorking(this,
                                                                         diagnosticLogger,
                                                                         sequenceContext,
                                                                         this._timeManager,
                                                                         outputTypeArg);

            return ValueTask.FromResult(new StageStepResult(outputType, output.GetTaskFrom(outputType)));
        }

        #endregion
    }
}
