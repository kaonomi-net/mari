// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Services
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Deferred;
    using MARI.Framework.Core.Abstractions.Exceptions;
    using MARI.Framework.Core.Abstractions.Services;
    using MARI.Framework.Core.Models;

    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Extensions;
    using MARI.Toolbox.Models;
    using MARI.Toolbox.Services;
    using MARI.Toolbox.Supports;
    using MARI.Toolbox.Tasks;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Newtonsoft.Json.Linq;

    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Client observer and deferred awaiter provider used to on client side await deferred work
    /// </summary>
    /// <seealso cref="IDeferredAwaiterHandler" />
    /// <seealso cref="IDeferredObserver" />
    internal sealed class DeferredClientObserver : SafeAsyncDisposable, IDeferredAwaiterHandler, IDeferredObserver, IInitService, IFinalizeService
    {
        #region Fields

        private readonly SupportInitializationImplementation<IServiceProvider> _initializer;
        private readonly SupportInitializationImplementation<IServiceProvider> _finalizer;
        private readonly DelayTimer _keepSubscriptionAliveDelayTimer;

        private readonly Dictionary<Guid, DeferredStatusMessage> _deferredStatus;
        private readonly Dictionary<Guid, ITaskCompletionSourceEx> _awaitingTasks;

        private readonly IVGrainMARISystemProvider _grainMARISystemProvider;
        private readonly IGrainOrleanFactory _grainOrleanFactory;
        private readonly ILogger<IDeferredObserver> _logger;

        private static readonly MethodInfo s_fetchGenericTaskResponse;

        private readonly SemaphoreSlim _subscriptionLocker;
        private readonly SemaphoreSlim _deferredLocker;

        private IDeferredObserver? _objRefObserver;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DeferredClientObserver"/> class.
        /// </summary>
        static DeferredClientObserver()
        {
            s_fetchGenericTaskResponse = ((MethodCallExpression)((LambdaExpression)((DeferredClientObserver c) => c.FetchTaskResponseAsync<int>(default, null))).Body).Method.GetGenericMethodDefinition();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredClientObserver"/> class.
        /// </summary>
        public DeferredClientObserver(ILogger<IDeferredObserver> logger,
                                      IGrainOrleanFactory grainOrleanFactory,
                                      IVGrainMARISystemProvider grainMARISystemProvider)
        {
            this._initializer = new SupportInitializationImplementation<IServiceProvider>(InitObserver);
            this._finalizer = new SupportInitializationImplementation<IServiceProvider>(FinalizeObserver);

            this._logger = logger ?? NullLogger<IDeferredObserver>.Instance;

            this._deferredStatus = new Dictionary<Guid, DeferredStatusMessage>();
            this._awaitingTasks = new Dictionary<Guid, ITaskCompletionSourceEx>();

            this._deferredLocker = new SemaphoreSlim(1);
            this._subscriptionLocker = new SemaphoreSlim(1);

            this._grainMARISystemProvider = grainMARISystemProvider;
            this._grainOrleanFactory = grainOrleanFactory;

            var refreshTimer = TimeSpan.FromMicroseconds(MARIConstants.DefaultObserverTimeout.TotalMicroseconds * 0.95);
            this._keepSubscriptionAliveDelayTimer = DelayTimer.Create(UpdateSubscription,
                                                                      tickDelay: refreshTimer,
                                                                      startDelay: refreshTimer,
                                                                      waitExecutionEnd: true);
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool ExpectOrleanStarted
        {
            get { return true; }
        }

        /// <inheritdoc />
        public bool IsInitializing
        {
            get { return this._initializer.IsInitializing; }
        }

        /// <inheritdoc />
        public bool IsInitialized
        {
            get { return this._initializer.IsInitialized; }
        }

        /// <inheritdoc />
        public bool IsFinalizing
        {
            get { return this._finalizer.IsInitializing; }
        }

        /// <inheritdoc />
        public bool IsFinalized
        {
            get { return this._finalizer.IsInitialized; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task DeferredStatusChangedNotification(DeferredStatusMessage statusChangeMessage)
        {
            var deferredId = statusChangeMessage.DeferredId.Uid;
            ITaskCompletionSourceEx? requiredResponse = null;

            using (await this._deferredLocker.LockAsync())
            {
                // Cleanup is broadcasted once the response is consumed (or the work cleaned) : always forget the work, even if never awaited by this client
                if (statusChangeMessage.Status == DeferredStatusEnum.Cleanup)
                {
                    this._awaitingTasks.Remove(deferredId);
                    this._deferredStatus.Remove(deferredId);
                    return;
                }

                this._deferredStatus[deferredId] = statusChangeMessage;

                if (this._awaitingTasks.TryGetValue(deferredId, out var pendingTask))
                {
                    if (statusChangeMessage.Status == DeferredStatusEnum.Cancelled || statusChangeMessage.Status == DeferredStatusEnum.Failed || statusChangeMessage.Status == DeferredStatusEnum.Finished)
                    {
                        requiredResponse = pendingTask;
                    }
                }
            }

            if (requiredResponse is not null)
            {
                StartFetchTaskResponse(statusChangeMessage, requiredResponse);
            }
        }

        /// <inheritdoc />
        public Task<IExecutionResult<TResponse>> GetDeferredWorkAwaiterAsync<TResponse>(DeferredId id, CancellationToken token)
        {
            return GetDeferredWorkAwaiterAsync<TResponse>(id.Uid, token);
        }

        /// <inheritdoc />
        public async Task<IExecutionResult<TResponse>> GetDeferredWorkAwaiterAsync<TResponse>(Guid id, CancellationToken token)
        {
            var task = await ReservedDeferredWorkSlotImpl<TResponse>(id, token);
            return await task;
        }

        /// <inheritdoc />
        public ValueTask InitializationAsync(IServiceProvider? initializationState, CancellationToken token = default)
        {
            return this._initializer.InitializationAsync(initializationState, token);
        }

        /// <inheritdoc />
        public ValueTask InitializationAsync(CancellationToken token = default)
        {
            return this._initializer.InitializationAsync(token);
        }

        /// <inheritdoc />
        public ValueTask FinalizeAsync(CancellationToken token = default)
        {
            return this._finalizer.InitializationAsync(token);
        }

        /// <inheritdoc />
        public async Task<Guid> ReservedDeferredWorkSlot<TResponse>(Guid? sourceId = null)
        {
            var handler = await this._grainMARISystemProvider.GetVGrainAsync<IDeferredHandlerVGrain>(null, this._logger);
            var workId = await handler.CreateDeferredWorkAsync(sourceId, null, (ConcretBaseType)typeof(TResponse).GetAbstractType());

            await ReservedDeferredWorkSlotImpl<TResponse>(workId);
            return workId;
        }

        /// <inheritdoc />
        public async Task CleanUpDeferredWork(Guid deferredId)
        {
            var handler = await this._grainMARISystemProvider.GetVGrainAsync<IDeferredHandlerVGrain>(null, this._logger);
            await handler.CleanUpDeferredWork(deferredId);
        }

        #region Tools

        /// <inheritdoc />
        public async Task<Task<IExecutionResult<TResponse>>> ReservedDeferredWorkSlotImpl<TResponse>(Guid id, CancellationToken? token = null)
        {
            Task<IExecutionResult<TResponse>>? task = null;
            DeferredStatusMessage? alreadyTerminatedStatus = null;
            ITaskCompletionSourceEx? alreadyTerminatedResponse = null;

            using (await this._deferredLocker.LockAsync())
            {
                token?.ThrowIfCancellationRequested();
                if (this._awaitingTasks.TryGetValue(id, out var pendingTask))
                {
                    task = (Task<IExecutionResult<TResponse>>)pendingTask.GetTask();
                }
                else
                {
                    DeferredStatusMessage? knownStatus = null;
                    if (this._deferredStatus.TryGetValue(id, out var deferredStatusMessage))
                    {
                        if (deferredStatusMessage.Status == DeferredStatusEnum.Cancelled)
                            throw new OperationCanceledException();

                        knownStatus = deferredStatusMessage;
                    }

                    var taskCompletion = new TaskCompletionSourceEx<IExecutionResult<TResponse>>(typeof(TResponse));
                    task = taskCompletion.Task;
                    this._awaitingTasks.Add(id, taskCompletion);

                    // Terminal notification already received before registration : fetch the response now instead of waiting a notification that will never come
                    if (knownStatus is not null &&
                        (knownStatus.Value.Status == DeferredStatusEnum.Finished || knownStatus.Value.Status == DeferredStatusEnum.Failed))
                    {
                        alreadyTerminatedStatus = knownStatus;
                        alreadyTerminatedResponse = taskCompletion;
                    }
                }
            }

            if (alreadyTerminatedStatus is not null && alreadyTerminatedResponse is not null)
                StartFetchTaskResponse(alreadyTerminatedStatus.Value, alreadyTerminatedResponse);

            if (token is not null && token.Value.CanBeCanceled && !task.IsCompleted)
            {
                var registration = token.Value.Register(uid => _ = CancelAwaitingTaskAsync((Guid)uid!), id, true);

                // Release the registration as soon as the awaited task is done to avoid rooting it on long-lived tokens
                _ = task.ContinueWith(static (_, reg) => ((CancellationTokenRegistration)reg!).Dispose(),
                                  registration,
                                  CancellationToken.None,
                                  TaskContinuationOptions.ExecuteSynchronously,
                                  TaskScheduler.Default);
            }

            return task;
        }

        /// <summary>
        /// Cancel and forget the awaiting task related to <paramref name="deferredTaskUid"/> without blocking the cancelling thread
        /// </summary>
        private async Task CancelAwaitingTaskAsync(Guid deferredTaskUid)
        {
            try
            {
                using (await this._deferredLocker.LockAsync())
                {
                    if (this._awaitingTasks.TryGetValue(deferredTaskUid, out var pendingTask))
                    {
                        this._awaitingTasks.Remove(deferredTaskUid);
                        pendingTask.TrySetCanceled();
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.OptiLog(LogLevel.Error, "Deferred awaiting task cancellation failed {deferredId} : {exception}", deferredTaskUid, ex);
            }
        }

        /// <summary>
        /// Start fetching the deferred response in background (not awaited to avoid blocking the notifying, non-reentrant, handler grain)
        /// while ensuring any failure is propagated to <paramref name="requiredResponse"/>.
        /// </summary>
        private void StartFetchTaskResponse(DeferredStatusMessage statusChangeMessage, ITaskCompletionSourceEx requiredResponse)
        {
            Task fetchTask;
            try
            {
                fetchTask = (Task)s_fetchGenericTaskResponse.MakeGenericMethod((Type)requiredResponse.State!).Invoke(this, new object?[] { statusChangeMessage, requiredResponse })!;
            }
            catch (Exception ex)
            {
                var rootEx = (ex as TargetInvocationException)?.InnerException ?? ex;
                this._logger.OptiLog(LogLevel.Error, "Deferred response fetch failed {deferredId} : {exception}", statusChangeMessage.DeferredId, rootEx);
                requiredResponse.TrySetException(rootEx);
                return;
            }

            fetchTask.ContinueWith((t, state) =>
            {
                var response = (ITaskCompletionSourceEx)state!;
                var ex = t.Exception!.InnerExceptions.Count == 1 ? t.Exception.InnerExceptions[0] : t.Exception;
                this._logger.OptiLog(LogLevel.Error, "Deferred response fetch failed {deferredId} : {exception}", statusChangeMessage.DeferredId, ex);
                response.TrySetException(ex);
            }, requiredResponse, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        /// <summary>
        /// Fetches pending <see cref="ITaskCompletionSourceEx"/> response
        /// </summary>
        private async Task FetchTaskResponseAsync<TExpectedResponse>(DeferredStatusMessage statusChangeMessage, ITaskCompletionSourceEx requiredResponse)
        {
            if (requiredResponse.GetTask().IsCompleted)
                return;

            var deferredId = statusChangeMessage.DeferredId.Uid;

            var handler = await this._grainMARISystemProvider.GetVGrainAsync<IDeferredHandlerVGrain>(null, this._logger);

            TExpectedResponse? result = default;
            MARIInternalException? internalException = null;

            if (statusChangeMessage.Status == DeferredStatusEnum.Failed)
            {
                internalException = await handler.ConsumeDeferredResponseAsync<MARIInternalException>(deferredId);
                internalException ??= new MARIInternalException("no exception",
                                                                     null!,
                                                                     MARIErrorCodes.Build(MARIErrorCodes.Categories.Execution,
                                                                                               MARIErrorCodes.PartType.Deferred,
                                                                                               MARIErrorCodes.ErrorType.Failed),
                                                                     null);
            }
            else if (statusChangeMessage.Status == DeferredStatusEnum.Finished)
            {
                result = await handler.ConsumeDeferredResponseAsync<TExpectedResponse>(deferredId);
            }

            var executionResult = new ExecutionResult<TExpectedResponse>(statusChangeMessage.DeferredId.SourceId,
                                                                         internalException is null,
                                                                         statusChangeMessage.Status == DeferredStatusEnum.Cancelled,
                                                                         internalException is not null ? MARIErrorCodes.ToDecryptErrorCode(internalException.ErrorCode) : string.Empty,
                                                                         internalException?.GetFullString(),
                                                                         result,
                                                                         hasOutput: statusChangeMessage.Status == DeferredStatusEnum.Finished && result is not null);

            requiredResponse.TrySetResultObject(executionResult);
        }

        /// <summary>
        /// 
        /// </summary>
        private async ValueTask InitObserver(IServiceProvider? provider, CancellationToken token)
        {
            await UpdateSubscription(token);
            await this._keepSubscriptionAliveDelayTimer.StartAsync(token);
        }

        /// <summary>
        /// 
        /// </summary>
        private async ValueTask FinalizeObserver(IServiceProvider? provider, CancellationToken token)
        {
            var timer = this._keepSubscriptionAliveDelayTimer;

            if (timer is not null && !this.IsDisposed)
                await this._keepSubscriptionAliveDelayTimer.StopAsync(token);
        }

        /// <summary>
        /// Update subscription to ObserverManager that is reset every <see cref="MARIConstants.DefaultObserverTimeout"/>
        /// </summary>
        private async Task UpdateSubscription(CancellationToken token)
        {
            await this._subscriptionLocker.WaitAsync(token);

            try
            {
                var handler = await this._grainMARISystemProvider.GetVGrainAsync<IDeferredHandlerVGrain>(null, this._logger);

                this._objRefObserver ??= this._grainOrleanFactory.CreateObjectReference<IDeferredObserver>(this);
                await handler.Subscribe(this._objRefObserver);
            }
            finally
            {
                this._subscriptionLocker.Release();
            }
        }

        /// <inheritdoc />
        protected override async ValueTask DisposeBeginAsync()
        {
            if (!this.IsFinalized)
                await FinalizeAsync(default);

            await base.DisposeBeginAsync();
        }

        #endregion

        #endregion
    }
}
