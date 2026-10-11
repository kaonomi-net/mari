// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Triggers
{
    using MARI.Framework.Core.Abstractions.Doors;
    using MARI.Framework.Core.Abstractions.Inputs;
    using MARI.Framework.Core.Abstractions.Signals;

    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [Immutable]
    [DataContract]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class SignalTriggerDefinition : TriggerDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalTriggerDefinition"/> class.
        /// </summary>
        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        public SignalTriggerDefinition(Guid uid,
                                       Uri refId,
                                       string displayName,
                                       IEnumerable<TriggerTargetDefinition> targets,
                                       bool enabled,
                                       SignalId? listenSignal,
                                       DoorId? listenDoor,
                                       DefinitionMetaData? metaData,
                                       DataSourceDefinition? triggerGlobalOutputDefinition = null)

            : base(uid, refId, displayName, TriggerTypeEnum.Signal, targets, enabled, metaData, triggerGlobalOutputDefinition)
        {
            this.ListenSignal = listenSignal;
            this.ListenDoor = listenDoor;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        [Id(0)]
        [DataMember]
        public SignalId? ListenSignal { get; }

        /// <inheritdoc />
        [Id(1)]
        [DataMember]
        public DoorId? ListenDoor { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override string OnDebugDisplayName()
        {
            return "[ListenSignal: " + this.ListenSignal + "] [ListenDoor: " + this.ListenDoor + "]";
        }

        /// <inheritdoc />
        protected override bool OnEquals(TriggerDefinition other)
        {
            return other is SignalTriggerDefinition otherSignal &&
                   object.Equals(this.ListenSignal, otherSignal.ListenSignal) &&
                   object.Equals(this.ListenDoor, otherSignal.ListenDoor);
        }

        /// <inheritdoc />
        protected override int OnGetHashCode()
        {
            return HashCode.Combine(this.ListenSignal, this.ListenDoor);
        }

        /// <inheritdoc />
        protected override bool OnValidate(ILogger logger, bool matchWarningAsError = false)
        {
            var valid = true;

            if (this.ListenDoor is null && this.ListenSignal is null)
            {
                logger.OptiLog(LogLevel.Error, "At least a door or a simple signal is required");
                valid = false;
            }

            return valid;
        }

        #endregion
    }
}
