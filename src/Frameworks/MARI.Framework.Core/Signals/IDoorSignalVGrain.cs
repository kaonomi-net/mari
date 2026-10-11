// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Signals
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Doors;
    using MARI.Framework.Core.Abstractions.Signals;

    using Orleans.Concurrency;

    /// <summary>
    /// VGrain in charge to managed output signal of the <see cref="IDoorVGrain"/>
    /// </summary>
    /// <seealso cref="ISignalHandler" />
    /// <seealso cref="IGrainWithGuidKey" />
    internal interface IDoorSignalVGrain : IVGrain, ISignalHandler, IGrainWithGuidKey
    {
        /// <summary>
        /// Fires a signal build by <see cref="DoorBaseVGrain"/>
        /// </summary>
        [OneWay]
        Task<Guid> Fire(SignalMessage signal);
    }
}
