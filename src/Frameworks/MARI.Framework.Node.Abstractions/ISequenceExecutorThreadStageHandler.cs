// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Diagnostics;
    using MARI.Framework.Core.Abstractions.Sequence;
    using MARI.Framework.Node.Abstractions.Models;
    using MARI.Toolbox.Abstractions.Disposables;

    using Microsoft.Extensions.Logging;

    using System.Threading.Tasks;

    /// <summary>
    /// Handler execution of the <see cref="ISequenceStageDefinition"/>
    /// </summary>
    internal interface ISequenceExecutorThreadStageHandler
    {
        /// <summary>
        /// Stage execution
        /// </summary>
        ValueTask<StageStepResult> ExecAsync(SequenceStageDefinition step,
                                             object? input,
                                             IExecutionContext sequenceContext,
                                             ILogger logger,
                                             IDiagnosticLogger diagnosticLogger,
                                             IVGrainProvider vgrainProvider, // Provider is inject to be able to change implementation at runtime
                                             Func<ISecureContextToken<ISequenceExecutorThreadHandler>> securityThreadHandlerAccessor);
    }
}
