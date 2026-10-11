// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using MARI.Framework.Cluster.Abstractions.Configurations.Builders;
    using MARI.Framework.Node.Abstractions.Configurations;
    using MARI.Framework.Node.Abstractions.Models;

    using System;

    /// <summary>
    ///  Cluster node configuration wizard tools
    /// </summary>
    public interface IMARINodeWizard : IMARIWizard<IMARINodeWizard, IMARINodeConfigurationWizard>, IBuilderMARIBaseWizard
    {
        #region Properties

        /// <summary>
        /// Gets the gneric builder tool.
        /// </summary>
        public IMARIExtensionBuilderTool ConfigurationTools { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Add a <see cref="INodeOptions"/>.
        /// </summary>
        IMARINodeWizard AddNodeOption<TOption>(TOption option) where TOption : class, INodeOptions;

        /// <summary>
        /// Add a <see cref="INodeOptions"/> using configuration section.
        /// </summary>
        IMARINodeWizard AddNodeOption<TOption>(string configurationSection) where TOption : class, INodeOptions;

        /// <summary>
        /// Manually setup the cluster vgrains
        /// </summary>
        IMARINodeWizard SetupNodeVGrains(Action<IMARINodeVGrainWizard> config);

        /// <summary>
        /// Setups in local memory definitions
        /// </summary>
        IMARINodeWizard AddInMemoryDefinitionProvider(Action<IMARINodeLocalDefinitionsBuilder> config);

        /// <summary>
        /// Setups the cluster options.
        /// </summary>
        IMARINodeWizard SetupClusterOptions(Action<IMARINodeClusterOptionWizard> action);

        /// <summary>
        /// Setups the nodes memories, how to same state, reminder, trigger ...
        /// </summary>
        IMARINodeWizard SetupNodeMemories(Action<IMARINodeMemoryBuilder> memoryBuilder);

        /// <summary>
        /// Setups the nodes memories, how to same state, reminder, trigger ...
        /// </summary>
        IMARINodeWizard SetupNodeMemories(string defaultAutokey, Action<IMARINodeMemoryBuilder>? memoryBuilder = null);

        #endregion
    }
}
