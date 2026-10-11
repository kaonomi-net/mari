// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// Keep : MARI.Framework.Configurations; to easy configuration use
namespace MARI.Framework.Configurations
{
    using MARI.Framework.Builders;
    using MARI.Framework.Cluster.Abstractions.Configurations.Builders;
    using MARI.Framework.Node.Blackboard.Abstractions;
    using MARI.Framework.Node.Blackboard.Abstractions.Models;
    using MARI.Framework.Node.Blackboard.Configurations;

    using Microsoft.Extensions.DependencyInjection;

    using System.Diagnostics;

    public static class MARINodeWizardBlackboardExtensions
    {
        #region Fields

        private static readonly string s_blackboardBuilderKey = nameof(MARINodeBlackboardsBuilder) + Guid.NewGuid().ToString();

        #endregion

        #region Methods

        /// <summary>
        /// Setups blackboards
        /// </summary>
        public static IMARINodeWizard UseBlackboards(this IMARINodeWizard wizard, Action<IMARINodeBlackboardsBuilder>? builder = null)
        {
            var blackboardAnalyzeFeedbackSignalDef = Signal.Create(BlackboardConstants.BlackboardAnalyzeFeedbackSignal, m => m.Description("Signal used as root for blackboard to get analyze result")
                                                                                                                              .AddTags("Feedback", "Blackboard")
                                                                                                                              .CategoryChain("Blackboard", "Signals", "Feedback"));

            wizard.AddInMemoryDefinitionProvider(p =>
            {
                p.SetupSignals(blackboardAnalyzeFeedbackSignalDef);
            });

            var bbBuilder = GetBlackboardBuilder(wizard.ConfigurationTools);
            builder?.Invoke(bbBuilder);
            return wizard;
        }

        /// <summary>
        /// Setups blackboards definitions
        /// </summary>
        public static IMARINodeLocalDefinitionsBuilder SetupBlackboardTemplates(this IMARINodeLocalDefinitionsBuilder wizard, params BlackboardTemplateDefinition[] templateDefinitions)
        {
            var bbBuilder = GetBlackboardBuilder(wizard.ConfigurationTools);
            bbBuilder.AddInMemoryDefinitionProvider(m => m.SetupTemplates(templateDefinitions));
            return wizard;
        }

        /// <summary>
        /// Gets the blackboard builder.
        /// </summary>
        private static MARINodeBlackboardsBuilder GetBlackboardBuilder(IMARIExtensionBuilderTool wizard)
        {
            var blackboardBuilder = wizard.GetServiceCollection().GetServiceByKey<string, MARINodeBlackboardsBuilder>(s_blackboardBuilderKey);

            if (blackboardBuilder is null)
            {
                var newBuilder = new MARINodeBlackboardsBuilder(wizard);
                wizard.GetServiceCollection().AddKeyedSingleton(s_blackboardBuilderKey, newBuilder);
                newBuilder.BuildServices();
                return newBuilder;
            }

            Debug.Assert(blackboardBuilder.KeyedImplementationInstance is not null);
            return (MARINodeBlackboardsBuilder)blackboardBuilder.KeyedImplementationInstance;
        }

        #endregion
    }
}