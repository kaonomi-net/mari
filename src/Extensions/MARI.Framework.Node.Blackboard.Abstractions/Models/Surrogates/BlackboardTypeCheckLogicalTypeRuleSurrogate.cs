// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions.Models.Surrogates
{
    using MARI.Framework.Core.Abstractions.Surrogates;
    using MARI.Framework.Node.Blackboard.Abstractions.Models.Rules;

    [GenerateSerializer]
    public record struct BlackboardTypeCheckLogicalTypeRuleSurrogate(string LogicalTypePattern,
                                                                     IConcretTypeSurrogate FilterType,
                                                                     IReadOnlyCollection<IBlackboardLogicalTypeBaseRuleSurrogate> Children) : IBlackboardLogicalTypeBaseRuleSurrogate;

    [RegisterConverter]
    public sealed class BlackboardTypeCheckLogicalTypeRuleConverter : IConverter<BlackboardTypeCheckLogicalTypeRule, BlackboardTypeCheckLogicalTypeRuleSurrogate>
    {
        /// <inheritdoc />
        public BlackboardTypeCheckLogicalTypeRule ConvertFromSurrogate(in BlackboardTypeCheckLogicalTypeRuleSurrogate surrogate)
        {
            return new BlackboardTypeCheckLogicalTypeRule(surrogate.LogicalTypePattern,
                                                          ConcretBaseTypeConverter.ConvertFromSurrogate(surrogate.FilterType),
                                                          BlackboardLogicalTypeBaseRuleSurrogateConverter.ConvertFromSurrogate(surrogate.Children));
        }

        /// <inheritdoc />
        public BlackboardTypeCheckLogicalTypeRuleSurrogate ConvertToSurrogate(in BlackboardTypeCheckLogicalTypeRule value)
        {
            return new BlackboardTypeCheckLogicalTypeRuleSurrogate()
            {
                LogicalTypePattern = value.LogicalTypePattern,
                FilterType = ConcretBaseTypeConverter.ConvertToSurrogate(value.FilterType),
                Children = BlackboardLogicalTypeBaseRuleSurrogateConverter.ConvertToSurrogate(value.Children).ToReadOnly()
            };
        }
    }
}
