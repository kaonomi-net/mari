// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.StreamQueue
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Triggers;
    using MARI.Framework.Node.Triggers;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;
    using Orleans.Services;

    /// <summary>
    /// Grain service in charge to handled trigger link to a stream
    /// </summary>
    /// <seealso cref="IGrainService" />
    public sealed class StreamTriggerVGrainService : TriggerBaseGrainService<IStreamTriggerHandlerVGrain>, IStreamTriggerVGrainService, ISiloStatusListener
    {
        #region Fields

        private readonly ISiloStatusOracle _statusOracle;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamTriggerVGrainService"/> class.
        /// </summary>
        public StreamTriggerVGrainService(GrainId id,
                                          Silo silo,
                                          ILoggerFactory loggerFactory,
                                          IGrainOrleanFactory grainFactory,
                                          ISiloStatusOracle statusOracle,
                                          ITriggerDefinitionProvider triggerDefinitionProvider)
            : base(id, silo, loggerFactory, grainFactory, triggerDefinitionProvider, TriggerTypeEnum.Stream)
        {
            this._statusOracle = statusOracle;
            statusOracle.SubscribeToSiloStatusEvents(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Receive notifications about silo status events.
        /// </summary>
        public void SiloStatusChangeNotification(SiloAddress updatedSilo, SiloStatus status)
        {
            if (this.Status == GrainServiceStatus.Booting || this.Status == GrainServiceStatus.Stopped || updatedSilo.IsClient || status != SiloStatus.Dead)
                return;

            // Refreshes are serialized by the base class lock; failures are logged instead of being unobserved
            _ = SafeRefreshInfoAsync();
        }

        /// <inheritdoc />
        public override Task Stop()
        {
            this._statusOracle.UnSubscribeFromSiloStatusEvents(this);
            return base.Stop();
        }

        /// <summary>
        /// Refresh information and log any failure
        /// </summary>
        private async Task SafeRefreshInfoAsync()
        {
            try
            {
                await RefreshInfoAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Stream trigger refresh after silo status change failed");
            }
        }

        #endregion
    }
}
