// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Configurations
{
    using MARI.Framework.Core.Abstractions.Enums;

    /// <summary>
    /// Builder used to configure all storage in cluster memory
    /// </summary>
    public interface IMARIMemoryInLocalBuilder
    {
        #region Methods

        /// <summary>
        /// Configure storage for <see cref="StorageTypeEnum"/>
        /// </summary>
        IMARIMemoryInLocalBuilder Store(StorageTypeEnum storageType);

        /// <summary>
        /// Get a storage dedicated to specific key.
        /// </summary>
        IMARIMemoryInLocalBuilder SetupGrainStateStorage(string key,
                                                              bool buildRepository = false);

        /// <summary>
        /// Get a storage dedicated to specific key.
        /// </summary>
        IMARIMemoryInLocalBuilder SetupGrainStateStorage(IReadOnlyCollection<string> keys,
                                                              bool buildRepository = false);

        /// <summary>
        /// Get a repository storage dedicated to specific key.
        /// </summary>
        IMARIMemoryInLocalBuilder SetupRepositoryStorage(string key);

        /// <summary>
        /// Get a repository storage dedicated to specific key.
        /// </summary>
        IMARIMemoryInLocalBuilder SetupRepositoryStorage(IReadOnlyCollection<string> keys);

        /// <summary>
        /// Setups the reminder storage.
        /// </summary>
        IMARIMemoryInLocalBuilder SetupDefaultStorage();

        /// <summary>
        /// Setups the reminder storage.
        /// </summary>
        IMARIMemoryInLocalBuilder SetupDefaultRepositoryStorage();

        /// <summary>
        /// Setups the mari dynamic definition storage.
        /// </summary>
        IMARIMemoryInLocalBuilder SetupDynamicDefinitionStorage();

        /// <summary>
        /// Setups the mari admin system storage.
        /// </summary>
        IMARIMemoryInLocalBuilder SetupMARIAdminStorage();

        /// <summary>
        /// Setups the mari system storage.
        /// </summary>
        IMARIMemoryInLocalBuilder SetupMARIStorage();

        /// <summary>
        /// Setups the reminder storage.
        /// </summary>
        IMARIMemoryInLocalBuilder SetupReminderStorage();

        #endregion
    }
}
