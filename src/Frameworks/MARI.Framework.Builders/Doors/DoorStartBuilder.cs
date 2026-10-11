// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.Doors
{
    using MARI.Framework.Builders.Signals;
    using MARI.Framework.Core.Abstractions.Doors;
    using MARI.Framework.Core.Abstractions.Exceptions;
    using MARI.Framework.Core.Abstractions.Signals;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Start door builder used to carry all default door information
    /// </summary>
    /// <seealso cref="IDoorBuilder" />
    internal sealed class DoorStartBuilder : SignalNetworkBasePartBuilder<IDoorBuilder>, IDoorBuilder, IDoorWithListenerBuilder
    {
        #region Fields

        private readonly HashSet<SignalId> _signalIds;
        private readonly HashSet<DoorId> _doorIds;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorStartBuilder"/> class.
        /// </summary>
        public DoorStartBuilder(string simpleNameIdentifier, string? displayName = null, Guid? uid = null, Action<IDefinitionMetaDataBuilder>? metaDataBuilder = null)
            : base(simpleNameIdentifier, displayName, uid, metaDataBuilder)
        {
            this._signalIds = new HashSet<SignalId>();
            this._doorIds = new HashSet<DoorId>();

            this.RetentionMaxPeriod = DoorDefinition.DEFAULT_RETENTION_MAX_DELAY;
            this.HistoryMaxRetention = DoorDefinition.DEFAULT_HISTORY_RETENTION;
            this.NotConsumedMaxRetention = DoorDefinition.DEFAULT_NOT_CONSUMED_RETENTION;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public TimeSpan? RetentionMaxPeriod { get; private set; }

        /// <inheritdoc />
        public uint? HistoryMaxRetention { get; private set; }

        /// <inheritdoc />
        public uint? NotConsumedMaxRetention { get; private set; }

        /// <inheritdoc />
        public IReadOnlyCollection<SignalId> SignalIds
        {
            get { return this._signalIds; }
        }

        /// <inheritdoc />
        public IReadOnlyCollection<DoorId> DoorIds
        {
            get { return this._doorIds; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IDoorBuilder SetSignalRetention(TimeSpan? retentionMaxPeriod)
        {
            this.RetentionMaxPeriod = retentionMaxPeriod;
            return this;
        }

        /// <inheritdoc />
        public IDoorBuilder SetSignalRetention(uint? history, uint? notConsumed)
        {
            this.HistoryMaxRetention = history;

            if (notConsumed != null && notConsumed < 1)
            {
                throw new InvalidDefinitionPropertyValueException(typeof(DoorDefinition),
                                                                  nameof(DoorDefinition.NotConsumedMaxRetention),
                                                                  notConsumed?.ToString() ?? string.Empty,
                                                                  "Couldn't be less than 1");
            }

            this.NotConsumedMaxRetention = notConsumed;
            return this;
        }

        /// <inheritdoc />
        public IDoorWithListenerBuilder Listen(params SignalDefinition[] signalDefinition)
        {
            return Listen(signalDefinition.Select(p => p.SignalId).ToArray());
        }

        /// <inheritdoc />
        public IDoorWithListenerBuilder Listen(params SignalId[] signalIds)
        {
            foreach (var signalId in signalIds)
                this._signalIds.Add(signalId);
            return this;
        }

        /// <inheritdoc />
        public IDoorWithListenerBuilder Listen(params DoorDefinition[] signalDefinition)
        {
            return Listen(signalDefinition.Select(p => p.DoorId).ToArray());
        }

        /// <inheritdoc />
        public IDoorWithListenerBuilder Listen(params DoorId[] doorIds)
        {
            foreach (var doorId in doorIds)
                this._doorIds.Add(doorId);
            return this;
        }

        #endregion
    }
}
