// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Executions
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Models;

    using MARI.Toolbox;
    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using Orleans.Concurrency;
    using Orleans.Runtime;

    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Launcher used to call a vgrain method and provide the result
    /// </summary>
    internal abstract class ExecutionBaseLauncher<TVGrain, TResult, TLauncher> : IExecutionLauncher, IExecutionLauncher<TResult>
        where TVGrain : IVGrain
    {
        #region Fields

        private static readonly MethodInfo s_genericCallMethod;

        private readonly IVGrainProvider _vgrainProvider;
        private readonly ILogger _logger;

        private IExecutionContext? _originContext;
        private bool _copyContextData;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ExecutionBaseLauncher{TResult}"/> class.
        /// </summary>
        static ExecutionBaseLauncher()
        {
            var trait = typeof(ExecutionBaseLauncher<TVGrain, TResult, TLauncher>);
            var genericCallMethod = trait.GetMethods()
                                         .FirstOrDefault(m => m.Name == nameof(IExecutionLauncher.RunAsync) &&
                                                              m.IsGenericMethod &&
                                                              m.GetParameters().Length == 1 &&
                                                              m.GetParameters().First().ParameterType == typeof(CancellationToken));

            Debug.Assert(genericCallMethod != null);
            s_genericCallMethod = genericCallMethod;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionBaseLauncher{TResult}"/> class.
        /// </summary>
        protected ExecutionBaseLauncher(ILogger? logger,
                                        IVGrainProvider vgrainProvider)
        {
            this._vgrainProvider = vgrainProvider;
            this._logger = logger ?? NullLogger.Instance;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        async Task<IExecutionResult> IExecutionLauncher.RunAsync(CancellationToken token)
        {
            return await ((IExecutionLauncher)this).RunAsync<NoneType>(token);
        }

        public Task<IExecutionResult> RunWithAnyResultAsync(CancellationToken token = default)
        {
            return RunImplAsync<IAnyType>(token);
        }

        /// <inheritdoc />
        Task<IExecutionResult<TResult>> IExecutionLauncher<TResult>.RunAsync(CancellationToken token)
        {
            return ((IExecutionLauncher)this).RunAsync<TResult>(token);
        }

        /// <inheritdoc />
        [OneWay]
        public Task Fire()
        {
            return FireImpl<NoneType>();
        }

        /// <inheritdoc />
        public async Task<IExecutionResult<TExpectedOutput>> RunAsync<TExpectedOutput>(CancellationToken token)
        {
            var result = await RunImplAsync<TExpectedOutput>(token);
            return (IExecutionResult<TExpectedOutput>)(result);
        }

        /// <inheritdoc />
        async Task<IExecutionResult> IExecutionLauncher.RunAsync(Type expectedOutput, CancellationToken token)
        {
            var specialisedMethod = s_genericCallMethod.MakeGenericMethod(expectedOutput);

            var task = (Task)specialisedMethod.Invoke(this, new object[] { token })!;

            await task;

            var taskResult = task.GetResult<IExecutionResult>();

            Debug.Assert(taskResult != null);

            return taskResult;
        }

        /// <inheritdoc />
        public TLauncher From(IExecutionContext executionContext, bool copyContextData = false)
        {
            this._originContext = executionContext;
            this._copyContextData = copyContextData;
            return GetLauncher();
        }

        #region Tools

        protected virtual TLauncher GetLauncher()
        {
            if (this is TLauncher launcher)
                return launcher;

            throw new InvalidOperationException("Must be overrided");
        }

        /// <inheritdoc />
        protected Task FireImpl<TExpectedResult>()
        {
            return Task.Factory.StartNew(async () =>
            {
                try
                {
                    await RunImplAsync<TExpectedResult>(default, true);
                }
                catch (Exception ex)
                {
                    this._logger.OptiLog(LogLevel.Error,
                                         "[{context}] Fire Task result on exception : {exception}",
                                         OnFireFailedBuildContext(ex),
                                         ex);
                }
            }).Unwrap();
        }

        /// <summary>
        /// Cancel the execution context and log (instead of throwing) any cancellation propagation failure
        /// </summary>
        private async Task SafeCancelAsync(IExecutionContext? executionContext)
        {
            if (executionContext is null)
                return;

            try
            {
                await executionContext.Cancel();
            }
            catch (Exception ex)
            {
                this._logger.OptiLog(LogLevel.Warning,
                                     "[{context}] Execution cancellation propagation failed : {exception}",
                                     executionContext.CurrentExecutionId,
                                     ex);
            }
        }

        /// <summary>
        ///     Generic execution able to call <typeparamref name="TExecutor"/> with diffent parameter
        /// </summary> 
        private async Task<IExecutionResult> RunImplAsync<TExpectedOutput>(CancellationToken token, bool fire = false)
        {
            var executionContext = GenerateExecutionContext(this._originContext?.FlowUID, this._originContext?.CurrentExecutionId);

            if (this._originContext is not null && this._copyContextData == true)
            {
                var data = this._originContext.GetAllDataContext() ?? EnumerableHelper<IContextDataContainer>.ReadOnly;

                foreach (var contextData in data.Where(d => d is not null))
                    executionContext.TryPushContextData(contextData, @override: false);
            }

            var input = GetInput();

            TVGrain executor;
            var forcedGrainId = GetForcedGrainId();

            if (forcedGrainId is not null)
            {
                if (this._vgrainProvider is not VGrainProvider vgrainProvider)
                    throw new NotSupportedException("Forcing vgrain id is not supported by the vgrain provider " + this._vgrainProvider?.GetType());

                executor = (TVGrain)vgrainProvider.GetVGrainWithForcedId(typeof(TVGrain), forcedGrainId.Value);
            }
            else
            {
                executor = await this._vgrainProvider.GetVGrainAsync<TVGrain>(input, executionContext, this._logger);
            }

            if (executionContext is IExecutionContextInternal executionContextInternal)
            {
                var grainRef = executor.AsReference<GrainReference>();
                executionContextInternal.AddCancelGrainReference(grainRef);
            }

            Exception? exception = null;
            Task? executionTask = null;
            try
            {
                token.ThrowIfCancellationRequested();

                // Link cancel event to orlean grain cancel system
                // Registration disposed once the execution is done to avoid rooting the context on long-lived tokens
                using (token.Register((ctx) => _ = SafeCancelAsync(ctx as IExecutionContext), executionContext, useSynchronizationContext: false))
                {
                    executionTask = OnRunAsync<TExpectedOutput>(executor, input, executionContext, fire);
                    await executionTask;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                await SafeCancelAsync(executionContext);
            }

            if (exception is null &&
                exception is not OperationCanceledException &&
                executionTask != null &&
                executionTask.Exception != null)
            {
                exception = executionTask.Exception;
            }

            object? output = default(TExpectedOutput);

            if (executionTask is not null && executionTask.IsCompletedSuccessfully)
            {
                if (AnyType.IsEqualTo<TExpectedOutput>())
                {
                    var result = executionTask?.GetResult();
                    var resultType = result?.GetType() ?? typeof(TExpectedOutput);

                    if (result is IAnyTypeContainer container)
                    {
                        result = container.GetData();
                        resultType = container.GetDataType();
                    }

                    // Call generic ExecutionResult.Create to build result based on the output
                    return ExecutionResult.CreateWithResult(executionContext,
                                                            result,
                                                            resultType,
                                                            exception,
                                                            succeeded: executionTask?.IsCompletedSuccessfully);
                }
                else if (executionTask.IsCompletedSuccessfully && !NoneType.IsEqualTo<TExpectedOutput>() && (executionTask?.IsCompletedSuccessfully ?? false))
                {
                    output = executionTask.GetResult<TExpectedOutput>();
                }
            }

            return ExecutionResult.Create<TExpectedOutput>(executionContext,
                                                           (TExpectedOutput?)output,
                                                           exception,
                                                           succeeded: executionTask?.IsCompletedSuccessfully);
        }

        /// <summary>
        /// Gets the input based on configuration.
        /// </summary>
        protected abstract object? GetInput();

        /// <summary>
        /// Gets the grain id the caller explicitly requested; null to let the provider build it from input and context.
        /// </summary>
        protected virtual IdSpan? GetForcedGrainId()
        {
            return null;
        }

        /// <summary>
        /// Generates <see cref="IExecutionContext"/> based on configuration.
        /// </summary>
        protected abstract IExecutionContext GenerateExecutionContext(Guid? flowId, Guid? parentId);

        /// <summary>
        /// Called to call the requested vgrain
        /// </summary>
        /// <returns></returns>
        protected abstract Task OnRunAsync<TExpectedOutput>(TVGrain executor, object? input, IExecutionContext executionContext, bool fire);

        /// <summary>
        /// Called when build context information to help identify the element that failed
        /// </summary>
        protected abstract string? OnFireFailedBuildContext(Exception ex);

        #endregion

        #endregion
    }
}
