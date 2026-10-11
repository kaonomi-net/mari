// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Signals.Doors
{
    using MARI.Framework.Core.Abstractions.Signals;
    using MARI.Framework.Node.Signals.Models;

    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Surrogate struct to serialize <see cref="DoorHandlerState"/>
    /// </summary>
    [GenerateSerializer]
    public struct DoorHandlerStateSurrogate
    {
        /// <summary>
        /// Gets or sets the signal states.
        /// </summary>
        [Id(0)]
        public IEnumerable<DoorSignalReceivedStatusSurrogate> SignalStates { get; set; }

        /// <summary>
        /// Gets or sets the listen signals.
        /// </summary>
        [Id(1)]
        public IEnumerable<Guid> ListenSignals { get; set; }
    }

    /// <summary>
    /// Declare a converter to convert both way <see cref="DoorHandlerState"/> to <see cref="DoorHandlerStateSurrogate"/>
    /// </summary>
    /// <seealso cref="IConverter{DoorHandlerState, DoorHandlerStateSurrogate}" />
    [RegisterConverter]
    public sealed class DoorHandlerStateSurrogateConverter : IConverter<DoorHandlerState, DoorHandlerStateSurrogate>
    {
        /// <inheritdoc />
        public DoorHandlerState ConvertFromSurrogate(in DoorHandlerStateSurrogate surrogate)
        {
            return new DoorHandlerState(surrogate.SignalStates, surrogate.ListenSignals);
        }

        /// <inheritdoc />
        public DoorHandlerStateSurrogate ConvertToSurrogate(in DoorHandlerState value)
        {
            return new DoorHandlerStateSurrogate()
            {
                SignalStates = value.SignalStatus
                                    .Select(DoorSignalReceivedStatusSurrogate.CreateFrom)
                                    .ToArray(),

                ListenSignals = value.ListenSignals.ToArray()
            };
        }
    }
}
