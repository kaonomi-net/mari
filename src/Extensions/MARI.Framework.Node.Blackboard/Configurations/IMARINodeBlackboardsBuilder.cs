// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Configurations
{
    using MARI.Framework.Node.Blackboard.Abstractions;

    /// <summary>
    /// Builder use to configure all the blackboard information
    /// </summary>
    public interface IMARINodeBlackboardsBuilder
    {
        /// <summary>
        /// Adds in memory definition related to blackboard.
        /// </summary>
        IMARINodeBlackboardsBuilder AddInMemoryDefinitionProvider(Action<IMARINodeBlackboardsLocalDefinitionBuilder> builder);

        /// <summary>
        /// Uses the default memory storage for blackboard registry state storage
        /// </summary>
        /// <remarks>
        ///     .AddMemoryGrainStorage(<see cref="BlackboardConstants.BlackboardRegistryStorageConfigurationKey"/>)
        /// </remarks>
        IMARINodeBlackboardsBuilder UseInMemoryStorageForRegistryState();

        /// <summary>
        /// Uses the default memory storage for <see cref="IBlackboardRef"/> state storage
        /// </summary>
        /// <remarks>
        ///     .AddMemoryGrainStorage(<see cref="BlackboardConstants.BlackboardStateStorageConfigurationKey"/>)
        /// </remarks>
        IMARINodeBlackboardsBuilder UseInMemoryStorageForBoardState();

        /// <summary>
        /// Uses the default memory storage for <see cref="IBlackboardRef"/> records storage
        /// </summary>
        /// <remarks>
        ///     .AddMemoryGrainStorage(<see cref="BlackboardConstants.BlackboardStorageRecordsConfigurationKey"/>)
        /// </remarks>
        IMARINodeBlackboardsBuilder UseInMemoryStorageForRecords();
    }
}
