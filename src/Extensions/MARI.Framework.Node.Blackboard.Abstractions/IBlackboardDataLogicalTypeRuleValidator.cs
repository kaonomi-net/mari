// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions
{
    using MARI.Toolbox.Abstractions.Enums;
    using MARI.Toolbox.Abstractions.Supports;

    /// <summary>
    /// Validate a record data
    /// </summary>
    public interface IBlackboardDataLogicalTypeRuleValidator : IBlackboardDataLogicalTypeRuleSolver, ISupportDebugDisplayName
    {
        /// <summary>
        /// Gets the collection group.
        /// </summary>
        string CollectionGroup { get; }

        /// <summary>
        /// Gets the validation mode.
        /// </summary>
        ValidationModeEnum ValidationMode { get; }
    }
}
