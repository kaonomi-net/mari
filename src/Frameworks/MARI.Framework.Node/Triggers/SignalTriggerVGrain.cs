// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Triggers
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Attributes;
    using MARI.Framework.Core.Abstractions.Signals;
    using MARI.Framework.Core.Abstractions.Streams;
    using MARI.Framework.Core.Abstractions.Triggers;
    using MARI.Framework.Node.Abstractions.Inputs;
    using MARI.Framework.Node.Abstractions.Triggers;

    using Microsoft.Extensions.Logging;

    using Orleans.Concurrency;
    using Orleans.Runtime;

    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Grain" />
    /// <seealso cref="ISignalTriggerVGrain" />
    [MARISystemVGrain]
    internal sealed class SignalTriggerVGrain : TriggerBaseHandlerVGrain<TriggerState, SignalTriggerDefinition, ISignalTriggerVGrain>, ISignalTriggerVGrain, ISignalReceiver, ISignalReceiverReadOnly
    {
        #region Fields

        private readonly ISignalService _localSignalService;
        private readonly SemaphoreSlim _fireLocker = new SemaphoreSlim(1, 1);

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalTriggerVGrain"/> class.
        /// </summary>
        public SignalTriggerVGrain(ILogger<ISignalTriggerVGrain> logger,
                                   [PersistentState(nameof(Triggers), MARIConstants.DefaultMARIStateConfigurationKey)] IPersistentState<TriggerState> persistentState,
                                   ITriggerDefinitionProvider triggerDefinitionProvider,
                                   ISequenceDefinitionProvider sequenceDefinitionProvider,
                                   ISignalDefinitionProvider signalDefinitionProvider,
                                   IMARIExecutionHandler mariExecutionHandler,
                                   IDataSourceProviderFactory inputSourceProviderFactory,
                                   IStreamQueueDefinitionProvider streamQueueDefinitionProvider,
                                   ISignalService signalService)
            : base(logger,
                   persistentState,
                   triggerDefinitionProvider,
                   sequenceDefinitionProvider,
                   signalDefinitionProvider,
                   mariExecutionHandler,
                   inputSourceProviderFactory,
                   streamQueueDefinitionProvider,
                   signalService)
        {
            this._localSignalService = signalService;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override async Task OnEnsureTriggerDefinitionAsync(CancellationToken token)
        {
            var definition = this.TriggerDefinition;

            if (definition.ListenSignal is not null)
                await this._localSignalService.SubscribeAsync(definition.ListenSignal.Value, this, token);
            
            if (definition.ListenDoor is not null)
                await this._localSignalService.SubscribeAsync(definition.ListenDoor.Value, this, token);
        }

        /// <inheritdoc />
        /// <remarks>
        ///     Called through <see cref="ISignalReceiverReadOnly"/> (interleaved calls),
        ///     signals are processed one by one to protect the trigger mutable state (data source providers, cursors, stream cache)
        /// </remarks>
        [OneWay]
        [ReadOnly]
        public async Task ReceiveSignalAsync(SignalMessage signal)
        {
            if (this.Enabled == false)
                return;

            await this._fireLocker.WaitAsync();
            try
            {
                await base.FireTriggerAsync(signal);
            }
            finally
            {
                this._fireLocker.Release();
            }
        }

        #endregion
    }
}
