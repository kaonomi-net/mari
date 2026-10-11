// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using MARI.Framework.Node.Abstractions.Models;

    /// <summary>
    /// Define the configuration section name used by mari's node
    /// </summary>
    public static class ConfigurationNodeSectionNames
    {
        /// <summary>
        /// The node specific section
        /// </summary>
        public const string NodeSpecificSection = "Node";

        /// <summary>
        /// The diagnostics section name configuration
        /// </summary>
        public const string Diagnostics = ConfigurationSectionNames.Root +
                                          ConfigurationSectionNames.SectionSeparator +
                                          NodeSpecificSection +
                                          ConfigurationSectionNames.SectionSeparator +
                                          ClusterNodeDiagnosticOptions.ConfigurationName;

        /// <summary>
        /// The root section in change to configured the storages memberships, vgrain states, reminder, timers ...
        /// </summary>
        public const string NodeStorages = ConfigurationSectionNames.Root + ConfigurationSectionNames.SectionSeparator + "Storages";

        /// <summary>
        /// The root section in change to configured the storages memberships, vgrain states, reminder, timers ...
        /// </summary>
        public const string NodeRepositoryStorages = ConfigurationSectionNames.Root + ConfigurationSectionNames.SectionSeparator + "Repositories";

        /// <summary>
        /// The root section in change to configured the storages memberships, vgrain states, reminder, timers ...
        /// </summary>
        public const string NodeRepositoryStoragesDefaultAutoConfigKey = NodeRepositoryStoragesDefault + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.AutoConfigKey;

        /// <summary>
        /// The root section in change to configured the storages memberships, vgrain states, reminder, timers ...
        /// </summary>
        public const string NodeRepositoryStoragesDefault = NodeRepositoryStorages + ConfigurationSectionNames.SectionSeparator + "Default";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeDefaultMemory = NodeStorages + ConfigurationSectionNames.SectionSeparator + "Default";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeDefaultMemoryAutoConfigKey = NodeDefaultMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.AutoConfigKey;

        /// <summary>
        /// The node default memory connection string
        /// </summary>
        public const string NodeDefaultMemoryConnectionString = NodeDefaultMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.ConnectionString;

        /// <summary>
        /// The section configuring auto key for all memory when misisin
        /// </summary>
        public const string NodeMemoryDefaultAutoConfigKey = NodeDefaultMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.AutoConfigKey;

        /// <summary>
        /// The section configuring the custom memory storage
        /// </summary>
        public const string NodeCustomMemory = NodeStorages + ConfigurationSectionNames.SectionSeparator + "Customs";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeMARIAdminMemory = NodeStorages + ConfigurationSectionNames.SectionSeparator + "Admin";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeMARIAdminMemoryAutoConfigKey = NodeMARIAdminMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.AutoConfigKey;

        /// <summary>
        /// The node mari system memory connection string
        /// </summary>
        public const string NodeMARIAdminMemoryConnectionString = NodeMARIAdminMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.ConnectionString;

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeMARIMemory = NodeStorages + ConfigurationSectionNames.SectionSeparator + "MARI";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeMARIMemoryAutoConfigKey = NodeMARIMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.AutoConfigKey;

        /// <summary>
        /// The node mari system memory connection string
        /// </summary>
        public const string NodeMARIMemoryConnectionString = NodeMARIMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.ConnectionString;

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeReminderStateMemory = NodeStorages + ConfigurationSectionNames.SectionSeparator + "Reminders";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeReminderStateMemoryConnectionString = NodeReminderStateMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.ConnectionString;

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeReminderStateMemoryAutoConfigKey = NodeReminderStateMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.AutoConfigKey;

        /// <summary>
        /// The root section in change to configured the definitions provider
        /// </summary>
        public const string NodeDefinitionProvider = ConfigurationSectionNames.Root + ConfigurationSectionNames.SectionSeparator + "Definitions";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeMARIDynamicDefinitionsMemory = NodeStorages + ConfigurationSectionNames.SectionSeparator + "DynamicDefinitions";

        /// <summary>
        /// The section configuring the vgrain state memory
        /// </summary>
        public const string NodeMARIDynamicDefinitionsMemoryAutoConfigKey = NodeMARIDynamicDefinitionsMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.AutoConfigKey;

        /// <summary>
        /// The node mari system memory connection string
        /// </summary>
        public const string NodeMARIDynamicDefinitionsMemoryConnectionString = NodeMARIDynamicDefinitionsMemory + ConfigurationSectionNames.SectionSeparator + ConfigurationSectionNames.ConnectionString;
    }
}
