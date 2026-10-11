// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions.Models.Surrogates
{
    using MARI.Framework.Node.Blackboard.Abstractions.Models.Rules;

    [GenerateSerializer]
    public record struct BlackboardOrderLogicalTypeRuleSurrogate(string LogicalTypePattern, short Order) : IBlackboardLogicalTypeBaseRuleSurrogate;

    [RegisterConverter]
    public sealed class BlackboardOrderLogicalTypeRuleConverter : IConverter<BlackboardOrderLogicalTypeRule, BlackboardOrderLogicalTypeRuleSurrogate>
    {
        /// <inheritdoc />
        public BlackboardOrderLogicalTypeRule ConvertFromSurrogate(in BlackboardOrderLogicalTypeRuleSurrogate surrogate)
        {
            return new BlackboardOrderLogicalTypeRule(surrogate.LogicalTypePattern, surrogate.Order);
        }

        /// <inheritdoc />
        public BlackboardOrderLogicalTypeRuleSurrogate ConvertToSurrogate(in BlackboardOrderLogicalTypeRule value)
        {
            return new BlackboardOrderLogicalTypeRuleSurrogate()
            {
                LogicalTypePattern = value.LogicalTypePattern,
                Order = value.Order
            };
        }
    }
}
