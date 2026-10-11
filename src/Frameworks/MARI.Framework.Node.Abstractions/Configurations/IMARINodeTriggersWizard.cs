// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using MARI.Framework.Core.Abstractions.Triggers;

    /// <summary>
    /// Setup local triggers
    /// </summary>
    public interface IMARINodeTriggersWizard
    {
        /// <summary>
        /// Registers a trigger definition.
        /// </summary>
        IMARINodeTriggersWizard Register(params TriggerDefinition[] triggerDefinition);
    }
}
