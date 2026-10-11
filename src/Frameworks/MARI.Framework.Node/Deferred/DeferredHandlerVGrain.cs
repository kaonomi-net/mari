// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Deferred
{
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Deferred;
    using MARI.Framework.Core.Abstractions.Exceptions;
    using MARI.Framework.Core.Abstractions.Repositories;

    using MARI.Toolbox.Abstractions.Services;
    using MARI.Toolbox.Models;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;
    using Orleans.Utilities;

    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Cluster deferred work handler
    /// </summary>
    /// <seealso cref="VGrainBase{IDeferredHandlerVGrain}" />
    /// <seealso cref="IDeferredHandlerVGrain" />
    [KeepAlive]
    internal sealed class DeferredHandlerVGrain : VGrainBase<DeferredHandlerState, DeferredHandlerStateSurrogate, DeferredHandlerStateConverter, IDeferredHandlerVGrain>, IDeferredHandlerVGrain
    {
        #region Fields

        private readonly ObserverManager<IDeferredObserver> _deferredObserverManager;
        private readonly IMARISerializer _mariSerializer;
        private readonly ITimeManager _timeManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredHandlerVGrain"/> class.
        /// </summary>
        public DeferredHandlerVGrain(ILogger<IDeferredHandlerVGrain> logger,
                                     [PersistentState("Deferred", MARIConstants.DefaultMARIStateConfigurationKey)] IPersistentState<DeferredHandlerStateSurrogate> persistentState,
                                     IMARISerializer mariSerializer,
                                     ITimeManager timeManager)
            : base(logger, persistentState)
        {
            this._deferredObserverManager = new ObserverManager<IDeferredObserver>(MARIConstants.DefaultObserverTimeout, logger);
            this._mariSerializer = mariSerializer;
            this._timeManager = timeManager;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<bool> CancelDeferredWorkStatusAsync(Guid deferredWorkUid, IIdentityCard identityCard)
        {
            if (identityCard is not null)
                throw new NotImplementedException("FEATURE to implement when the security through identityCard is builded");

            var work = this.State!.GetDeferredWorkById(deferredWorkUid);

            if (work is null || (work.Status != DeferredStatusEnum.Initialize && work.Status != DeferredStatusEnum.Alive))
                return false;

            work.ChangeStatus(DeferredStatusEnum.Cancelled, this._timeManager);
            await PushStateAsync(default);

            await SendNotificationAsync(work);

            return true;
        }

        /// <inheritdoc />
        public async Task CleanUpDeferredWork(Guid deferredId)
        {
            var work = this.State!.GetDeferredWorkById(deferredId);

            if (work is not null)
                await CleanUpDeferredWorkImpl(work);
        }

        /// <inheritdoc />
        public async Task<TResponse?> ConsumeDeferredResponseAsync<TResponse>(Guid deferredWorkUid)
        {
            var work = this.State!.GetDeferredWorkById(deferredWorkUid);

            if (work is null || (work.Status != DeferredStatusEnum.Failed && work.Status != DeferredStatusEnum.Cancelled && work.Status != DeferredStatusEnum.Finished))
                throw new MARIException($"Deferred work doesn't exist or is not done yet : {deferredWorkUid}");

            this.State!.Remove(work);
            await PushStateAsync(default);

            await CleanUpDeferredWorkImpl(work);

            if (work.Response is null)
                return default;

            if (work.Status == DeferredStatusEnum.Failed)
                throw this._mariSerializer.Deserialize<MARIInternalException>(work.Response);

            return this._mariSerializer.Deserialize<TResponse>(work.Response);
        }

        /// <inheritdoc />
        public async Task<Guid> CreateDeferredWorkAsync(Guid? sourceId, IIdentityCard identityCard, ConcretBaseType expectedResponseType, DateTime? utcEndValidity = null)
        {
            var newWork = new DeferredWork(new DeferredId(Guid.NewGuid(), sourceId ?? Guid.Empty), DeferredStatusEnum.Initialize, null, expectedResponseType, this._timeManager.UtcNow);
            this.State!.Add(newWork);
            await this.PushStateAsync(default);

            await SendNotificationAsync(newWork);

            return newWork.DeferredId.Uid;
        }

        /// <inheritdoc />
        public async Task<bool> ExceptionDeferredWorkStatusAsync(Guid deferredWorkUid, IIdentityCard identityCard, MARIInternalException exception)
        {
            // TODO : Check security : identityCard
            if (identityCard is not null)
                throw new NotImplementedException("FEATURE to implement when the security through identityCard is builded");

            var work = this.State!.GetDeferredWorkById(deferredWorkUid);

            if (work is null || (work.Status != DeferredStatusEnum.Initialize && work.Status != DeferredStatusEnum.Alive))
                return false;

            var exceptionBinary = this._mariSerializer.SerializeToBinary(exception);
            work.SetResult(exceptionBinary, this._timeManager, DeferredStatusEnum.Failed);
            await this.PushStateAsync(default);

            await SendNotificationAsync(work);

            return true;
        }

        /// <inheritdoc />
        public Task<bool> FinishDeferredWorkWithResultAsync(Guid deferredWorkUid, IIdentityCard identityCard, IExecutionResult response)
        {
            if (response.Cancelled)
                return CancelDeferredWorkStatusAsync(deferredWorkUid, identityCard);

            if (!response.Succeeded)
            {
                return ExceptionDeferredWorkStatusAsync(deferredWorkUid,
                                                        identityCard,
                                                        new MARIException("[ErrorCode:{0}] - Message: {1}".WithArguments(response.ErrorCode, response.Message)).ToMARIInternal());
            }

            return FinishDeferredWorkWithDataAsync(deferredWorkUid, identityCard, response.GetOutput());
        }

        /// <inheritdoc />
        public async Task<bool> FinishDeferredWorkWithDataAsync<TData>(Guid deferredWorkUid, IIdentityCard identityCard, TData response)
        {
            // TODO : Check security : identityCard
            if (identityCard is not null)
                throw new NotImplementedException("FEATURE to implement when the security through identityCard is builded");

            var work = this.State!.GetDeferredWorkById(deferredWorkUid);

            if (work is null || (work.Status != DeferredStatusEnum.Initialize && work.Status != DeferredStatusEnum.Alive))
                return false;

            work.SetResult(this._mariSerializer.SerializeToBinary(response).ToArray(), this._timeManager);
            await this.PushStateAsync(default);

            await SendNotificationAsync(work);

            return true;
        }

        /// <inheritdoc />
        public Task<DeferredStatusMessage?> GetLastDeferredStatusAsync(Guid deferredId)
        {
            var work = this.State!.GetDeferredWorkById(deferredId);

            DeferredStatusMessage? deferredStatusMessage = null;

            if (work is not null)
                deferredStatusMessage = new DeferredStatusMessage(work.DeferredId, work.Status, work.UTCLastUpdate);
            return Task.FromResult(deferredStatusMessage);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<DeferredStatusMessage>> GetLastDeferredStatusByEmitterAsync(IIdentityCard? identityCardFilter = null)
        {
            if (identityCardFilter is not null)
                throw new NotImplementedException("FEATURE to implement when the security through identityCard is builded");

            return Task.FromResult<IReadOnlyCollection<DeferredStatusMessage>>(EnumerableHelper<DeferredStatusMessage>.ReadOnlyArray);
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<DeferredStatusMessage>> GetLastDeferredStatusBySourceIdAsync(Guid sourceId, IIdentityCard? identityCardFilter = null)
        {
            if (identityCardFilter is not null)
                throw new NotImplementedException("FEATURE to implement when the security through identityCard is builded");

            var works = this.State!.GetDeferredWorkBySourceId(sourceId, identityCardFilter);

            var results = works?.Select(w => new DeferredStatusMessage(w.DeferredId, w.Status, w.UTCLastUpdate)).ToArray() ?? EnumerableHelper<DeferredStatusMessage>.ReadOnly;
            return Task.FromResult(results);
        }

        /// <inheritdoc />
        public async Task<bool> KeepDeferredWorkStatusAsync(Guid deferredWorkUid, IIdentityCard identityCard)
        {
            // TODO : Check security : identityCard
            if (identityCard is not null)
                throw new NotImplementedException("FEATURE to implement when the security through identityCard is builded");

            var work = this.State!.GetDeferredWorkById(deferredWorkUid);

            if (work is null)
                return false;

            work.ChangeStatus(DeferredStatusEnum.Alive, this._timeManager);
            await this.PushStateAsync(default);

            await SendNotificationAsync(work);

            return true;
        }

        /// <inheritdoc />
        public Task Subscribe(IDeferredObserver deferredObserver)
        {
            this._deferredObserverManager.Subscribe(deferredObserver, deferredObserver);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task Unsubscribe(IDeferredObserver deferredObserver)
        {
            this._deferredObserverManager.Unsubscribe(deferredObserver);
            return Task.CompletedTask;
        }

        #region Tools

        /// <summary>
        /// Sends the notification asynchronous.
        /// </summary>
        private async Task SendNotificationAsync(DeferredWork work)
        {
            // Optim : Add predicate filter

            var msg = new DeferredStatusMessage(work.DeferredId, work.Status, work.UTCLastUpdate);

            await this._deferredObserverManager.Notify(d => d.DeferredStatusChangedNotification(msg));
        }

        /// <summary>
        /// Mark the work as cleaned up and notify observers.
        /// </summary>
        private async Task CleanUpDeferredWorkImpl(DeferredWork work)
        {
            work.ChangeStatus(DeferredStatusEnum.Cleanup, this._timeManager);
            await SendNotificationAsync(work);
        }

        #endregion

        #endregion
    }
}
;