// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Diagnostics;
    using MARI.Framework.Core.Diagnostics;
    using MARI.Toolbox.Abstractions.Models;
    using MARI.Toolbox.Abstractions.Services;

    internal static class SequenceExecutorThreadStageHandlerExtensions
    {
        /// <summary>
        /// Build start stage log
        /// </summary>
        internal static void StartStageWorking(this ISequenceExecutorThreadStageHandler _,
                                               IDiagnosticLogger diagnosticLogger,
                                               IExecutionContext sequenceContext,
                                               ITimeManager timeManager,
                                               TypedArgument? input)
        {

            var inputDiagnosticLog = new DiagnosticInOutLog(sequenceContext.FlowUID,
                                                            sequenceContext.CurrentExecutionId,
                                                            sequenceContext.ParentExecutionId,
                                                            Core.Abstractions.Enums.OrientationEnum.In,
                                                            timeManager.UtcNow,
                                                            input,
                                                            string.Empty);
            diagnosticLogger.Log(inputDiagnosticLog);
        }

        /// <summary>
        /// Build start end log
        /// </summary>
        internal static void EndStageWorking(this ISequenceExecutorThreadStageHandler _,
                                             IDiagnosticLogger diagnosticLogger,
                                             IExecutionContext sequenceContext,
                                             ITimeManager timeManager,
                                             TypedArgument? output)
        {

            var outputDiagnosticLog = new DiagnosticInOutLog(sequenceContext.FlowUID,
                                                             sequenceContext.CurrentExecutionId,
                                                             sequenceContext.ParentExecutionId,
                                                             Core.Abstractions.Enums.OrientationEnum.Out,
                                                             timeManager.UtcNow,
                                                             output,
                                                             string.Empty);
            diagnosticLogger.Log(outputDiagnosticLog);
        }
    }
}
