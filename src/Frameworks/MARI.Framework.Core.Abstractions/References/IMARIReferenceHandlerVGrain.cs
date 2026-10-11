// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.References
{
    using MARI.Framework.Core.Abstractions.Attributes;

    using Orleans.Concurrency;

    /// <summary>
    /// VGrain in charge to execute cluster side <see cref="IMARIExecutionHandler"/> call with references
    /// </summary>
    /// <remarks>
    ///     MARI References need to be on cluster side 
    /// </remarks>
    /// <seealso cref="IVGrain" />
    [VGrainStatelessWorker]
    [MARISystemVGrain]
    internal interface IMARIReferenceHandlerVGrain : IVGrain, IGrainWithIntegerKey
    {
        #region Methods

        /// <inheritdoc cref="ICommonExecutionLauncher{IExecutionRefLauncher}.Fire" />
        [OneWay]
        [ReadOnly]
        Task Fire<TRefExecCommand>(TRefExecCommand execCommand)
            where TRefExecCommand : IRefExecuteCommand;

        /// <inheritdoc cref="IExecutionRefLauncher.FireWithDeferred{TResult}" />
        [ReadOnly]
        Task<Guid> FireWithDeferred<TResult, TRefExecCommand>(TRefExecCommand execCommand)
            where TRefExecCommand : IRefExecuteCommand;

        /// <inheritdoc cref="IExecutionLauncher.RunAsync(GrainCancellationToken)" />
        [ReadOnly]
        Task<IExecutionResult> RunAsync<TRefExecCommand>(TRefExecCommand execCommand, GrainCancellationToken token)
            where TRefExecCommand : IRefExecuteCommand;

        /// <inheritdoc cref="IExecutionLauncher.RunAsync{TExpectedOutput}(CancellationToken)" />
        [ReadOnly]
        Task<IExecutionResult<TExpectedOutput>> RunAsync<TExpectedOutput, TRefExecCommand>(TRefExecCommand execCommand, GrainCancellationToken token)
            where TRefExecCommand : IRefExecuteCommand;

        /// <inheritdoc cref="IExecutionLauncher.RunWithAnyResultAsync(CancellationToken)" />
        [ReadOnly]
        Task<IExecutionResult> RunWithAnyResultAsync<TRefExecCommand>(TRefExecCommand execCommand, GrainCancellationToken token)
            where TRefExecCommand : IRefExecuteCommand;

        // TODO : Ask Definition Guid based on refId

        #endregion
    }
}
