// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions.Models
{
    using MARI.Toolbox.Abstractions.Supports;

    using System.Collections.Generic;

    /// <summary>
    /// Data conflict information
    /// </summary>
    [GenerateSerializer]
    public record class DataRecordConflict(IReadOnlyCollection<DataRecordContainer> ConflictItems, DataRecordConflictReasonEnum Reason, string? Details) : ISupportDebugDisplayName
    {
        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return $"[{this.Reason}] ({this.ConflictItems.Count}) {this.Details}";
        }
    }
}
