// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions.Models.Surrogates
{
    using MARI.Framework.Node.Blackboard.Abstractions.Models.Rules;
    using System.Text.RegularExpressions;

    [GenerateSerializer]
    public record struct BlackboardRegexLogicalTypeRuleSurrogate(string LogicalTypePattern,
                                                                 string MatchRegex,
                                                                 RegexOptions RegexOptions,
                                                                 IReadOnlyCollection<IBlackboardLogicalTypeBaseRuleSurrogate> Children) : IBlackboardLogicalTypeBaseRuleSurrogate;

    [RegisterConverter]
    public sealed class BlackboardRegexLogicalTypeRuleConverter : IConverter<BlackboardRegexLogicalTypeRule, BlackboardRegexLogicalTypeRuleSurrogate>
    {
        /// <inheritdoc />
        public BlackboardRegexLogicalTypeRule ConvertFromSurrogate(in BlackboardRegexLogicalTypeRuleSurrogate surrogate)
        {
            return new BlackboardRegexLogicalTypeRule(surrogate.LogicalTypePattern, surrogate.MatchRegex, surrogate.RegexOptions);
        }

        /// <inheritdoc />
        public BlackboardRegexLogicalTypeRuleSurrogate ConvertToSurrogate(in BlackboardRegexLogicalTypeRule value)
        {
            return new BlackboardRegexLogicalTypeRuleSurrogate()
            {
                LogicalTypePattern = value.LogicalTypePattern,
                MatchRegex = value.MatchRegex,
                RegexOptions = value.RegexOptions,
            };
        }
    }
}
