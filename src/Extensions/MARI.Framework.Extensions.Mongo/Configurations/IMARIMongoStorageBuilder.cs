// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Extensions.Mongo.Configurations
{
    using MARI.Framework.Configurations;
    using MARI.Framework.Core.Abstractions.Enums;
    using MARI.Framework.Extensions.Mongo.Services;

    using MongoDB.Bson.Serialization;

    using Orleans.Providers.MongoDB.Configuration;

    /// <summary>
    /// Configurator dedicated to mongo db storage
    /// </summary>
    public interface IMARIMongoStorageBuilder
    {
        #region Methods

        /// <summary>
        /// Set the connection information state.
        /// </summary>
        IMARIMongoStorageBuilder ConnectionString(string connectionString,
                                                             string defaultDatabase,
                                                             string? collectionPrefix = null);

        /// <summary>
        /// Configure storage for <see cref="StorageTypeEnum"/>
        /// </summary>
        IMARIMongoStorageBuilder Store(StorageTypeEnum storageType,
                                            string? connectionString = null,
                                            string? customDatabase = null,
                                            string? collectionPrefix = null);

        /// <summary>
        /// Get a storage dedicated to specific key.
        /// </summary>
        IMARIMongoStorageBuilder SetupGrainStateStorage(string key,
                                                             string? connectionString = null,
                                                             string? customDatabase = null,
                                                             string? collectionPrefix = null,
                                                             bool buildRepository = false);

        /// <summary>
        /// Get a storage dedicated to specific key.
        /// </summary>
        IMARIMongoStorageBuilder SetupGrainStateStorage(IReadOnlyCollection<string> keys,
                                                             string? connectionString = null,
                                                             string? customDatabase = null,
                                                             string? collectionPrefix = null,
                                                             bool buildRepository = false);

        /// <summary>
        /// Get a repository storage dedicated to specific key.
        /// </summary>
        IMARIMongoStorageBuilder SetupRepositoryStorage(string key,
                                                             string? connectionString = null,
                                                             string? customDatabase = null,
                                                             string? collectionPrefix = null);

        /// <summary>
        /// Get a repository storage dedicated to specific key.
        /// </summary>
        IMARIMongoStorageBuilder SetupRepositoryStorage(IReadOnlyCollection<string> keys,
                                                             string? connectionString = null,
                                                             string? customDatabase = null,
                                                             string? collectionPrefix = null);

        /// <summary>
        /// Adds a bson convert.
        /// </summary>
        IMARIMongoStorageBuilder AddConvert<TTarget>(IBsonSerializer<TTarget> bsonSerializer);

        /// <summary>
        /// Setups the reminder storage.
        /// </summary>
        IMARIMongoStorageBuilder SetupDefaultStorage(string? connectionString = null, string? customDatabase = null, string? collectionPrefix = null);

        /// <summary>
        /// Setups the reminder storage.
        /// </summary>
        IMARIMongoStorageBuilder SetupDefaultRepositoryStorage(string? connectionString = null, string? customDatabase = null, string? collectionPrefix = null);

        /// <summary>
        /// Setups the mari dynamic definition storage.
        /// </summary>
        IMARIMongoStorageBuilder SetupDynamicDefinitionStorage(string? connectionString = null, string? customDatabase = null, string? collectionPrefix = null);

        /// <summary>
        /// Setups the mari admin system storage.
        /// </summary>
        IMARIMongoStorageBuilder SetupMARIAdminStorage(string? connectionString = null, string? customDatabase = null, string? collectionPrefix = null);

        /// <summary>
        /// Setups the mari system storage.
        /// </summary>
        IMARIMongoStorageBuilder SetupMARIStorage(string? connectionString = null, string? customDatabase = null, string? collectionPrefix = null);

        /// <summary>
        /// Setups the reminder storage.
        /// </summary>
        IMARIMongoStorageBuilder SetupReminderStorage(string? connectionString = null, string? customDatabase = null, string? collectionPrefix = null);

        /// <summary>
        /// Setups the reminder storage.
        /// </summary>
        IMARIMongoStorageBuilder SetupDefinitionStorage(string key, string? connectionString = null, string? customDatabase = null, string? collectionPrefix = null);

        /// <summary>
        /// Setups the mongo cluster.
        /// </summary>
        void SetupMongoCluster(IMARIClusterBuilder cl,
                               string? connectionString = null,
                               MongoDBMembershipTableOptions? option = null,
                               string? serviceId = null,
                               string? clusterId = null);

        #endregion
    }
}
