// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.Triggers
{
    using MARI.Framework.Core.Abstractions.Triggers;

    /// <summary>
    /// Common part of trigger definition
    /// </summary>
    public interface ITriggerDefinitionFinalizeBuilder : ITriggerDefinitionBuilder, IDefinitionBaseBuilder<TriggerDefinition>
    {
    }
}
