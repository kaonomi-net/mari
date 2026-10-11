// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions.Models.Surrogates
{
    using MARI.Framework.Node.Blackboard.Abstractions.Models.Rules;

    [GenerateSerializer]
    public record struct BlackboardStorageLogicalTypeRuleSurrogate(string LogicalTypePattern, BlackboardStorageDefinition Storage) : IBlackboardLogicalTypeBaseRuleSurrogate;

    [RegisterConverter]
    public sealed class BlackboardStorageLogicalTypeRuleConverter : IConverter<BlackboardStorageLogicalTypeRule, BlackboardStorageLogicalTypeRuleSurrogate>
    {
        /// <inheritdoc />
        public BlackboardStorageLogicalTypeRule ConvertFromSurrogate(in BlackboardStorageLogicalTypeRuleSurrogate surrogate)
        {
            return new BlackboardStorageLogicalTypeRule(surrogate.LogicalTypePattern, surrogate.Storage);
        }

        /// <inheritdoc />
        public BlackboardStorageLogicalTypeRuleSurrogate ConvertToSurrogate(in BlackboardStorageLogicalTypeRule value)
        {
            return new BlackboardStorageLogicalTypeRuleSurrogate()
            {
                LogicalTypePattern = value.LogicalTypePattern,
                Storage = value.Storage
            };
        }
    }
}
