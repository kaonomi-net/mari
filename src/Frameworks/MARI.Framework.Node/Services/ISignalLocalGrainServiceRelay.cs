// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Services
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Signals;

    using Orleans.Services;

    /// <summary>
    /// 
    /// </summary>
    internal interface ISignalLocalGrainServiceRelay : IGrainService, ISignalReceiver, ISignalReceiverReadOnly
    {
        /// <summary>
        /// Subscribes the specified action to be called when <paramref name="signalId"/> is emit
        /// </summary>
        Task<SubscriptionId> Subscribe(SignalId signalId);

        /// <summary>
        /// Unsubscribe
        /// </summary>
        Task Unsubscribe(SubscriptionId subscriptionId);
    }

    /// <summary>
    /// 
    /// </summary>
    internal interface ISignalLocalGrainServiceRelayClient : IGrainServiceClient<ISignalLocalGrainServiceRelay>, ISignalLocalGrainServiceRelay
    {
    }
}
