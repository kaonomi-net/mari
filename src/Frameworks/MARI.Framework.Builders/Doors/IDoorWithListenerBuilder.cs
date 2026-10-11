// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.Doors
{
    using MARI.Framework.Builders.Signals;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Doors;
    using MARI.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Builder of <see cref="DoorDefinition"/>
    /// </summary>
    /// <seealso cref="ISignalNetworkBasePartBuilder{ISignalBuilder}" />
    public interface IDoorWithListenerBuilder : IDoorBuilder
    {
        #region Properties

        /// <summary>
        /// Gets unique door name
        /// </summary>
        string? DisplayName { get; }

        /// <summary>
        /// Gets the simple name identifier.
        /// </summary>
        string SimpleNameIdentifier { get; }    

        /// <summary>
        /// Gets the unique id.
        /// </summary>
        Guid Uid { get; }

        /// <summary>
        /// Gets the definition meta data.
        /// </summary>
        DefinitionMetaData? DefinitionMetaData { get; }

        /// <summary>
        /// Gets the global signal retention period (history and not consumed), Default 1 day
        /// </summary>
        TimeSpan? RetentionMaxPeriod { get; }

        /// <summary>
        /// Gets the signal history maximum retention. Default 0 to prevent any signal history
        /// </summary>
        uint? HistoryMaxRetention { get; }

        /// <summary>
        /// Gets the signal "not consumed" maximum retention. Default null to keep all the incoming signals
        /// </summary>
        uint? NotConsumedMaxRetention { get; }

        /// <summary>
        /// Gets the signal "not consumed" maximum retention.
        /// </summary>
        [Obsolete("Misspelled, use " + nameof(NotConsumedMaxRetention) + " instead.")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        uint? NotConsumedMaxRetiention
        {
            get { return this.NotConsumedMaxRetention; }
        }

        /// <summary>
        /// Gets the signal ids.
        /// </summary>
        IReadOnlyCollection<SignalId> SignalIds { get; }

        /// <summary>
        /// Gets the door ids.
        /// </summary>
        IReadOnlyCollection<DoorId> DoorIds { get; }

        #endregion
    }
}
