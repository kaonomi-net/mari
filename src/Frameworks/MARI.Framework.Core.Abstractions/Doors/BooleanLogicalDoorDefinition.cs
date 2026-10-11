// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Doors
{
    using MARI.Framework.Core.Abstractions.Signals;

    using MARI.Toolbox.Extensions;
    using MARI.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Aggreagtor door that use a logical formula to define if synpase fire or not
    /// </summary>
    /// <seealso cref="DoorDefinition" />
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class BooleanLogicalDoorDefinition : DoorDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanLogicalDoorDefinition"/> class.
        /// </summary>
        public BooleanLogicalDoorDefinition(Guid uid,
                                            Uri refId,
                                            string name,
                                            string logicalFormula,
                                            string vgrainInterfaceFullName,
                                            IEnumerable<SignalId>? signalSourceIds,
                                            IEnumerable<DoorId>? doorSourceIds,
                                            Dictionary<string, Guid> variableNames,
                                            bool useCurrentDoorStatus,
                                            DefinitionMetaData? metaData,
                                            TimeSpan? activeWindowInterval = null,
                                            TimeSpan? retentionMaxDelay = null,
                                            uint? historyMaxRetention = null,
                                            uint? notConsumedMaxRetiention = null)
            : base(uid, refId, name, vgrainInterfaceFullName, signalSourceIds, doorSourceIds, metaData, activeWindowInterval, retentionMaxDelay, historyMaxRetention, notConsumedMaxRetiention)
        {
            ArgumentException.ThrowIfNullOrEmpty(logicalFormula);

            this.LogicalFormula = logicalFormula;
            this.UseCurrentDoorStatus = useCurrentDoorStatus;
            this.VariableNames = variableNames ?? DictionaryHelper<string, Guid>.ReadOnly;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the logical formula.
        /// </summary>
        [Id(0)]
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        public string LogicalFormula { get; }

        /// <summary>
        /// Gets the variable names.
        /// </summary>
        [Id(1)]
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        public IReadOnlyDictionary<string, Guid> VariableNames { get; }

        /// <summary>
        /// Gets a value indicating whether use current door status.
        /// </summary>
        [Id(2)]
        [DataMember]
        [Newtonsoft.Json.JsonProperty]
        public bool UseCurrentDoorStatus { get; }

        #endregion

        #region Methods

        /// <inheritdoc />>
        protected override bool OnDoorChildValidate(ILogger logger, bool matchErrorAsWarning)
        {
            var isValid = true;

            if (string.IsNullOrEmpty(this.LogicalFormula))
            {
                logger.OptiLog(LogLevel.Critical, "LogicalFormula MUST not be null or empty");
                isValid = false;
            }

            return isValid;
        }

        /// <inheritdoc />
        protected override bool OnDoorEquals(DoorDefinition otherDoor)
        {
            return otherDoor is BooleanLogicalDoorDefinition otherLogicalDoor &&
                   this.LogicalFormula == otherLogicalDoor.LogicalFormula &&
                   VariableNamesEquals(this.VariableNames, otherLogicalDoor.VariableNames) &&
                   this.UseCurrentDoorStatus == otherLogicalDoor.UseCurrentDoorStatus;
        }

        /// <inheritdoc />
        protected override int OnDoorGetHashCode()
        {
            var variableNamesHash = 0;
            foreach (var kv in this.VariableNames)
                variableNamesHash ^= HashCode.Combine(kv.Key, kv.Value);

            return HashCode.Combine(this.LogicalFormula, variableNamesHash, this.UseCurrentDoorStatus);
        }

        /// <summary>
        /// Compare variable names dictionaries content, independently of the entries order
        /// </summary>
        private static bool VariableNamesEquals(IReadOnlyDictionary<string, Guid> source, IReadOnlyDictionary<string, Guid> other)
        {
            if (object.ReferenceEquals(source, other))
                return true;

            if (source.Count != other.Count)
                return false;

            foreach (var kv in source)
            {
                if (!other.TryGetValue(kv.Key, out var otherValue) || otherValue != kv.Value)
                    return false;
            }

            return true;
        }

        #endregion
    }
}
