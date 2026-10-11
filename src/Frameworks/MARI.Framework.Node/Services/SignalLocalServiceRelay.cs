// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Services
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Signals;
    using MARI.Framework.Node.Abstractions.Services;

    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ISignalLocalGrainServiceRelay&gt;" />
    internal sealed class SignalLocalServiceRelay : ISignalLocalServiceRelay, ISignalReceiver
    {
        #region Fields

        private readonly ISignalLocalGrainServiceRelayClient _signalLocalGrainServiceRelayClient;
        private readonly SemaphoreSlim _subscribeLocker;
        private readonly ILogger _logger;

        private readonly Dictionary<Guid, Task<SubscriptionId>> _signalSubscribe;
        private readonly Dictionary<Guid, CallbackInfo> _signalSubscribeCallback;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalLocalServiceRelay"/> class.
        /// </summary>
        public SignalLocalServiceRelay(ISignalLocalGrainServiceRelayClient signalLocalGrainServiceRelayClient,
                                       ILogger<ISignalLocalServiceRelay> logger)
        {
            this._signalLocalGrainServiceRelayClient = signalLocalGrainServiceRelayClient;
            this._logger = logger;
            this._signalSubscribeCallback = new Dictionary<Guid, CallbackInfo>();
            this._signalSubscribe = new Dictionary<Guid, Task<SubscriptionId>>();

            this._subscribeLocker = new SemaphoreSlim(1);
        }

        #endregion

        #region Nested

        private sealed record class CallbackInfo(Guid Uid, Guid SignalId, Func<SignalMessage, ValueTask> Action, Predicate<SignalMessage>? Predicated);

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task ReceiveSignalAsync(SignalMessage signal)
        {
            var callbacks = EnumerableHelper<CallbackInfo>.ReadOnly;

            await this._subscribeLocker.WaitAsync();
            try
            {
                callbacks = this._signalSubscribeCallback.Where(s => s.Value.SignalId == signal.From.SourceDefinitionId && (s.Value.Predicated is null || s.Value.Predicated(signal)))
                                                         .Select(kv => kv.Value)
                                                         .ToArray();
            }
            finally
            {
                this._subscribeLocker.Release();
            }

            List<Exception>? exceptions = null;
            foreach (var callback in callbacks)
            {
                try
                {
                    await callback.Action(signal);
                }
                catch (Exception ex)
                {
                    exceptions ??= new List<Exception>();
                    exceptions.Add(ex);
                }
            }

            if (exceptions is not null && exceptions.Any())
                throw new AggregateException(exceptions);
        }

        /// <inheritdoc />
        public async ValueTask<IDisposable> SubscribeAsync(Func<SignalMessage, ValueTask> action, SignalId signalId, Predicate<SignalMessage>? predicated = null)
        {
            await this._subscribeLocker.WaitAsync();
            try
            {
                Task<SubscriptionId>? subscriptionIdTask = null;

                if (!this._signalSubscribe.TryGetValue(signalId.Uid, out subscriptionIdTask))
                {
                    subscriptionIdTask = this._signalLocalGrainServiceRelayClient.Subscribe(signalId);
                    this._signalSubscribe.Add(signalId.Uid, subscriptionIdTask);

                    // Evict the subscription if it failed, to let the next subscriber retry
                    _ = EvictSubscriptionOnFaultAsync(signalId.Uid, subscriptionIdTask);
                }

                var subscription = new CallbackInfo(Guid.NewGuid(), signalId.Uid, action, predicated);
                this._signalSubscribeCallback.Add(subscription.Uid, subscription);

                return new DisposableAction<Guid>(Unsubscribe, subscription.Uid);
            }
            finally
            {
                this._subscribeLocker.Release();
            }
        }

        #region Tools

        /// <summary>
        /// Unsubscribes the specified unique identifier.
        /// </summary>
        private void Unsubscribe(Guid subscriptionId)
        {
            Task<SubscriptionId>? unsubscribeTask = null;
            this._subscribeLocker.Wait();
            try
            {
                if (this._signalSubscribeCallback.Remove(subscriptionId, out var callbackInfo))
                {
                    var existingSubscriptions = this._signalSubscribeCallback.Any(s => s.Value.SignalId == callbackInfo.SignalId);

                    if (existingSubscriptions == false && this._signalSubscribe.Remove(callbackInfo.SignalId, out var signalSubscriptionTask))
                    {
                        unsubscribeTask = signalSubscriptionTask;
                    }
                }
            }
            finally
            {
                this._subscribeLocker.Release();
            }

            if (unsubscribeTask is not null)
            {
                // Attention to dead lock : never block, the remote unsubscribe is chained on the subscription task (even if still pending)
                _ = UnsubscribeRemoteAsync(unsubscribeTask);
            }
        }

        /// <summary>
        /// Wait for the remote subscription and then remove it; failures are logged.
        /// </summary>
        private async Task UnsubscribeRemoteAsync(Task<SubscriptionId> subscriptionTask)
        {
            SubscriptionId subscriptionId;
            try
            {
                subscriptionId = await subscriptionTask.ConfigureAwait(false);
            }
            catch
            {
                // Subscription failed, nothing to unsubscribe (failure already logged by EvictSubscriptionOnFaultAsync)
                return;
            }

            try
            {
                await this._signalLocalGrainServiceRelayClient.Unsubscribe(subscriptionId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this._logger.OptiLog(LogLevel.Error, "Signal relay unsubscribe failed {subscriptionId} : {exception}", subscriptionId, ex);
            }
        }

        /// <summary>
        /// Remove the cached subscription task if it fails.
        /// </summary>
        private async Task EvictSubscriptionOnFaultAsync(Guid signalUid, Task<SubscriptionId> subscriptionTask)
        {
            try
            {
                await subscriptionTask.ConfigureAwait(false);
                return;
            }
            catch (Exception ex)
            {
                this._logger.OptiLog(LogLevel.Error, "Signal relay subscription failed for signal {signalUid} : {exception}", signalUid, ex);
            }

            await this._subscribeLocker.WaitAsync().ConfigureAwait(false);
            try
            {
                if (this._signalSubscribe.TryGetValue(signalUid, out var current) && ReferenceEquals(current, subscriptionTask))
                    this._signalSubscribe.Remove(signalUid);
            }
            finally
            {
                this._subscribeLocker.Release();
            }
        }

        #endregion

        #endregion
    }
}
