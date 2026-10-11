// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace Kaonomi.Sample.Forex.VGrain.Abstractions
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Doors;
    using MARI.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// VGrain used to suscribe to signal or doord to test 
    /// </summary>
    /// <seealso cref="IVGrain" />
    public interface ISignalDemoTargetVGrain : IVGrain, ISignalReceiver, IGrainWithGuidKey
    {
        Task SubscribeToAsync(SignalDefinition signalDefinition);

        Task SubscribeToAsync(DoorDefinition doorDefinition);
    }
}
