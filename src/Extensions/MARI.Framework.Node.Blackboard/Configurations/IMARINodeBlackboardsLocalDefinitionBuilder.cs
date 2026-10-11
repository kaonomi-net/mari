// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Configurations
{
    using MARI.Framework.Node.Blackboard.Abstractions.Models;

    /// <summary>
    /// Builder use to configure in memory blackboard template definition
    /// </summary>
    public interface IMARINodeBlackboardsLocalDefinitionBuilder
    {
        /// <summary>
        /// Adds the templates.
        /// </summary>
        IMARINodeBlackboardsLocalDefinitionBuilder SetupTemplates(params BlackboardTemplateDefinition[] templates);
    }
}
