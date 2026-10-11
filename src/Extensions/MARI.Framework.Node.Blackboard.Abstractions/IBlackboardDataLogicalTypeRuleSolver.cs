// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions
{
    using MARI.Framework.Node.Blackboard.Abstractions.Models;
    using MARI.Framework.Node.Blackboard.Abstractions.Models.Issues;

    /// <summary>
    /// Validate a record data
    /// </summary>
    public interface IBlackboardDataLogicalTypeRuleSolver
    {
        /// <summary>
        /// Validates <paramref name="data"/> in the context of the blacboard with all the rule setups.
        /// </summary>
        /// <returns>
        ///     Issue found, stop at 1st the founded; I no issue return null;
        /// </returns>
        ValueTask<BlackboardProcessingIssue?> ValidateAsync<TData>(DataRecordContainer<TData> data,
                                                                   IReadOnlyDictionary<Guid, BlackboardRecordMetadata> recordMetadata);
    }
}
