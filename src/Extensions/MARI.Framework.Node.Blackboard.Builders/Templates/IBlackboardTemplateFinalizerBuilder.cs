// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Builders.Templates
{
    using MARI.Framework.Builders;
    using MARI.Framework.Node.Blackboard.Abstractions.Models;

    /// <summary>
    /// Configuration finalizer 
    /// </summary>
    public interface IBlackboardTemplateFinalizerBuilder : IDefinitionBaseBuilder<BlackboardTemplateDefinition>
    {
        /// <summary>
        /// Configures the default storage.
        /// </summary>
        IBlackboardTemplateFinalizerBuilder SetupDefaultStorage(string storageKey, string? storageConfiguration = null);

        /// <summary>
        /// Logicals the type configuration.
        /// </summary>
        IBlackboardTemplateFinalizerBuilder LogicalTypeConfiguration(string logicRecordTypePattern, Action<ILogicalTypeConfiguration> cfg);

        /// <summary>
        /// Logicals the type configuration.
        /// </summary>
        IBlackboardTemplateFinalizerBuilder AnyLogicalTypeConfiguration(Action<ILogicalTypeConfiguration>? cfg = null);
    }
}
