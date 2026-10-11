// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions.Models.Surrogates
{
    using MARI.Framework.Node.Blackboard.Abstractions.Models.Rules;

    [GenerateSerializer]
    public record struct BlackboardLogicalTypeUniqueRuleSurrogate(string LogicalTypePattern, bool AllowReplacement) : IBlackboardLogicalTypeBaseRuleSurrogate;

    [RegisterConverter]
    public sealed class BlackboardLogicalTypeUniqueRuleConverter : IConverter<BlackboardLogicalTypeUniqueRule, BlackboardLogicalTypeUniqueRuleSurrogate>
    {
        /// <inheritdoc />
        public BlackboardLogicalTypeUniqueRule ConvertFromSurrogate(in BlackboardLogicalTypeUniqueRuleSurrogate surrogate)
        {
            return new BlackboardLogicalTypeUniqueRule(surrogate.LogicalTypePattern, surrogate.AllowReplacement);
        }

        /// <inheritdoc />
        public BlackboardLogicalTypeUniqueRuleSurrogate ConvertToSurrogate(in BlackboardLogicalTypeUniqueRule value)
        {
            return new BlackboardLogicalTypeUniqueRuleSurrogate()
            {
                LogicalTypePattern = value.LogicalTypePattern,
                AllowReplacement = value.AllowReplacement
            };
        }
    }
}
