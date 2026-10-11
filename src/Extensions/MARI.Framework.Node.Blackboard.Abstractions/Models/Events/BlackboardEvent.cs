// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions.Models.Events
{
    using MARI.Toolbox.Abstractions.Supports;

    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    /// <summary>
    /// Base class of every event a blackboard can produce
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    [DebuggerDisplay("{ToDebugDisplayName()}")]
    public abstract record class BlackboardEvent(BlackboardEventTypeEnum EventType) : ISupportDebugDisplayName
    {
        /// <inheritdoc />
        public abstract string ToDebugDisplayName();
    }

    /// <summary>
    /// Base class of every event related to storage a blackboard can produce
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class BlackboardEventStorage(BlackboardEventStorageTypeEnum Action, Guid EntityId, BlackboardRecordMetadata? Metadata) : BlackboardEvent(BlackboardEventTypeEnum.Storage)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.EventType}] - Storage {this.Action} - {this.Metadata?.DisplayName}";
        }
    }

    /// <summary>
    /// Blackboard event based on life status change
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed record class BlackboardEventLifeStatusChanged(BlackboardLifeStatusEnum NewStatus, BlackboardLifeStatusEnum OldStatus) : BlackboardEvent(BlackboardEventTypeEnum.LifeStatusChanged)
    {
        /// <inheritdoc />
        public override string ToDebugDisplayName()
        {
            return $"[{this.EventType}] - Life Status {this.OldStatus} -> {this.NewStatus}";
        }
    }
}
