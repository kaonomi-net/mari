// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Extensions.Mongo.Configurations
{
    using MARI.Framework.Configurations;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Enums;
    using MARI.Framework.Core.Abstractions.Storages;
    using MARI.Framework.Core.Repositories;
    using MARI.Framework.Extensions.Mongo.Abstractions;
    using MARI.Framework.Extensions.Mongo.Models;
    using MARI.Framework.Extensions.Mongo.Repositories;
    using MARI.Framework.Extensions.Mongo.Services;

    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    using MongoDB.Bson.Serialization;

    using Orleans.Providers;
    using Orleans.Providers.MongoDB.Configuration;
    using Orleans.Providers.MongoDB.Membership;
    using Orleans.Providers.MongoDB.StorageProviders.Serializers;
    using Orleans.Providers.MongoDB.Utils;

    /// <summary>
    /// Configurator dedicated to mongo db storage
    /// </summary>
    internal sealed class MARIMongoStorageBuilder : IMARIMongoStorageBuilder
    {
        #region Fields

        private readonly IMARIBaseGenericBuilder _mariBaseGenericBuilder;
        private readonly IServiceCollection _serviceCollection;
        private readonly IConfiguration _configuration;

        private MongoDBConnectionOptions _lastConnectionConfiguration;
        private int _connectionInfoOrder;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MARIMongoStorageBuilder"/> class.
        /// </summary>
        public MARIMongoStorageBuilder(IServiceCollection serviceCollection, IConfiguration configuration, IMARIBaseGenericBuilder mariBaseGenericBuilder)
        {
            this._mariBaseGenericBuilder = mariBaseGenericBuilder;
            this._serviceCollection = serviceCollection;
            this._configuration = configuration;

            serviceCollection.AddSingleton<IMongoClientFactory, MultipleMongoClientFactory>();
            serviceCollection.AddSingleton<IGrainStateSerializer, BsonGrainStateSerializer>();

            serviceCollection.TryAddSingleton<MongoRepositoryDefinitionFactory>();
            serviceCollection.TryAddSingleton<MongoRepositoryGrainStateFactory>();
            serviceCollection.TryAddSingleton<MongoRepositoryFactory>();

            serviceCollection.AllowResolvingKeyedServicesAsDictionary();
            serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IDefinitionSourceProvider<>), typeof(MongoDefinitionProviderMultiSource<>)));

            MARIMongoDefaultSerializerConfig.SetupSerializationConfiguration();

            // Define default values
            this._lastConnectionConfiguration = new MongoDBConnectionOptions()
            {
                DatabaseName = nameof(MARI).ToLower(),
                ConnectionString = "127.0.0.1:27017",
                Order = -1
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set the connection information state.
        /// </summary>
        public IMARIMongoStorageBuilder ConnectionString(string connectionString,
                                                              string defaultDatabase,
                                                              string? collectionPrefix = null)
        {
            if (!string.IsNullOrEmpty(connectionString))
                this._lastConnectionConfiguration.ConnectionString = connectionString;

            if (!string.IsNullOrEmpty(defaultDatabase))
                this._lastConnectionConfiguration.DatabaseName = defaultDatabase;

            if (!string.IsNullOrEmpty(collectionPrefix))
                this._lastConnectionConfiguration.CollectionPrefix = collectionPrefix;

            return this;
        }

        /// <summary>
        /// Get a storage dedicated to <see cref="StorageTypeEnum"/>.
        /// </summary>
        public IMARIMongoStorageBuilder Store(StorageTypeEnum storageType,
                                                   string? connectionString = null,
                                                   string? customDatabase = null,
                                                   string? collectionPrefix = null)
        {
            if ((storageType & StorageTypeEnum.Default) == StorageTypeEnum.Default)
            {
                SetupDefaultStorage(connectionString, customDatabase, collectionPrefix);
                storageType -= StorageTypeEnum.Default;
            }

            if ((storageType & StorageTypeEnum.MARI) == StorageTypeEnum.MARI)
            {
                SetupMARIStorage(connectionString, customDatabase, collectionPrefix);
                storageType -= StorageTypeEnum.MARI;
            }

            if ((storageType & StorageTypeEnum.DynamicDefinition) == StorageTypeEnum.DynamicDefinition)
            {
                SetupDynamicDefinitionStorage(connectionString, customDatabase, collectionPrefix);
                storageType -= StorageTypeEnum.DynamicDefinition;
            }

            if ((storageType & StorageTypeEnum.MARIAdmin) == StorageTypeEnum.MARIAdmin)
            {
                SetupMARIAdminStorage();
                storageType -= StorageTypeEnum.MARIAdmin;
            }

            if ((storageType & StorageTypeEnum.Reminders) == StorageTypeEnum.Reminders)
            {
                SetupReminderStorage(connectionString, customDatabase, collectionPrefix);
                storageType -= StorageTypeEnum.Reminders;
            }

            if ((storageType & StorageTypeEnum.Repositories) == StorageTypeEnum.Repositories)
            {
                SetupDefaultRepositoryStorage(connectionString, customDatabase, collectionPrefix);
                storageType -= StorageTypeEnum.Repositories;
            }

            if (storageType != 0)
                throw new InvalidOperationException("Mongo Storage configuration not managed : " + storageType);

            return this;
        }

        /// <summary>
        /// Setups the mari dynamic definition storage.
        /// </summary>
        public IMARIMongoStorageBuilder SetupDynamicDefinitionStorage(string? connectionString = null,
                                                                           string? customDatabase = null,
                                                                           string? collectionPrefix = null)
        {
            var key = MARIConstants.DefaultMARIDynamicDefinitionsConfigurationKey;
            var optKey = ConfigurationNodeSectionNames.NodeMARIDynamicDefinitionsMemory;
            var connectionStringOptKey = ConfigurationNodeSectionNames.NodeMARIDynamicDefinitionsMemoryConnectionString;

            SetupGrainStateStorageImpl<MongoDBOptions>(key,
                                                       connectionString,
                                                       customDatabase,
                                                       collectionPrefix,
                                                       optKey,
                                                       connectionStringOptKey,
                                                       false);

            SetupRepositoryStorage(MARIConstants.DefaultMARIDynamicDefinitionsRepositoryConfigurationKey,
                                   connectionString,
                                   customDatabase,
                                   collectionPrefix);

            return this;
        }

        /// <summary>
        /// Setups the reminder storage.
        /// </summary>
        public IMARIMongoStorageBuilder SetupMARIStorage(string? connectionString = null,
                                                                   string? customDatabase = null,
                                                                   string? collectionPrefix = null)
        {
            var key = MARIConstants.DefaultMARIStateConfigurationKey;
            var optKey = ConfigurationNodeSectionNames.NodeMARIMemory;
            var connectionStringOptKey = ConfigurationNodeSectionNames.NodeMARIMemoryConnectionString;

            SetupGrainStateStorageImpl<MongoDBOptions>(key,
                                                       connectionString,
                                                       customDatabase,
                                                       collectionPrefix,
                                                       optKey,
                                                       connectionStringOptKey,
                                                       false);

            return this;
        }

        /// <summary>
        /// Setups the default grain storage.
        /// </summary>
        public IMARIMongoStorageBuilder SetupDefaultStorage(string? connectionString = null,
                                                                 string? customDatabase = null,
                                                                 string? collectionPrefix = null)
        {
            var key = ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME;
            var optKey = ConfigurationNodeSectionNames.NodeDefaultMemoryAutoConfigKey;
            var connectionStringOptKey = ConfigurationNodeSectionNames.NodeDefaultMemoryConnectionString;

            SetupGrainStateStorageImpl<MongoDBOptions>(key,
                                                       connectionString,
                                                       customDatabase,
                                                       collectionPrefix,
                                                       optKey,
                                                       connectionStringOptKey,
                                                       false);

            SetupGrainStateStorage("Storage");

            return this;
        }

        /// <summary>
        /// Setups the default grain storage.
        /// </summary>
        public IMARIMongoStorageBuilder SetupDefaultRepositoryStorage(string? connectionString = null,
                                                                           string? customDatabase = null,
                                                                           string? collectionPrefix = null)
        {
            var key = MARIConstants.DefaultMARIRepositoryConfigurationKey;
            var optKey = ConfigurationNodeSectionNames.NodeRepositoryStoragesDefault;
            var connectionStringOptKey = ConfigurationNodeSectionNames.NodeRepositoryStoragesDefaultAutoConfigKey;

            SetupRepositoryStorageImpl<MongoDBOptions>(key,
                                                       connectionString,
                                                       customDatabase,
                                                       collectionPrefix,
                                                       optKey,
                                                       connectionStringOptKey);

            return this;
        }

        /// <summary>
        /// Setups the mari admin storage.
        /// </summary>
        public IMARIMongoStorageBuilder SetupMARIAdminStorage(string? connectionString = null,
                                                                        string? customDatabase = null,
                                                                        string? collectionPrefix = null)
        {
            var key = MARIConstants.DefaultMARIAdminStateConfigurationKey;
            var optKey = ConfigurationNodeSectionNames.NodeMARIAdminMemory;
            var connectionStringOptKey = ConfigurationNodeSectionNames.NodeMARIAdminMemoryConnectionString;

            SetupGrainStateStorageImpl<MongoDBOptions>(key,
                                                       connectionString,
                                                       customDatabase,
                                                       collectionPrefix,
                                                       optKey,
                                                       connectionStringOptKey,
                                                       false);
            return this;
        }

        /// <summary>
        /// Setups the reminder storage.
        /// </summary>
        public IMARIMongoStorageBuilder SetupReminderStorage(string? connectionString = null, string? customDatabase = null, string? collectionPrefix = null)
        {
            var option = BuildConnectionOption<MongoDBRemindersOptions>(MongoDBConnectionOptions.MARI_REMINDER,
                                                                        connectionString,
                                                                        customDatabase,
                                                                        collectionPrefix,
                                                                        ConfigurationNodeSectionNames.NodeReminderStateMemory,
                                                                        ConfigurationNodeSectionNames.NodeReminderStateMemoryConnectionString,
                                                                        out var connectionOption);

            var siloBuilder = this._mariBaseGenericBuilder.SourceOrleanBuilder as ISiloBuilder;

#pragma warning disable IDE0270 // Use coalesce expression
            if (siloBuilder == null)
                throw new InvalidOperationException("The auto configurator must only be used by Node/Server side");
#pragma warning restore IDE0270 // Use coalesce expression

            var services = siloBuilder.Services;

            SetupOptions(MongoDBConnectionOptions.MARI_REMINDER, option, connectionOption, services);

            services.TryAddSingleton(option.ToOption());
            services.TryAddSingleton(option.ToMonitorOption());

            siloBuilder.UseMongoDBReminders(o =>
            {
                o.DatabaseName ??= o.DatabaseName;
                o.ClientName ??= o.ClientName;
                o.CollectionPrefix ??= o.CollectionPrefix;
            });

            return this;
        }

        /// <summary>
        /// Get a storage dedicated to specific key.
        /// </summary>
        public IMARIMongoStorageBuilder SetupGrainStateStorage(string key,
                                                                    string? connectionString = null,
                                                                    string? customDatabase = null,
                                                                    string? collectionPrefix = null,
                                                                    bool buildRepository = false)
        {
            var optKey = ConfigurationNodeSectionNames.NodeCustomMemory +
                         ConfigurationSectionNames.SectionSeparator +
                         key;

            var configConnectionString = ConfigurationNodeSectionNames.NodeCustomMemory +
                                         ConfigurationSectionNames.SectionSeparator +
                                         key +
                                         ConfigurationSectionNames.SectionSeparator +
                                         ConfigurationSectionNames.ConnectionString;

            SetupGrainStateStorageImpl<MongoDBOptions>(key,
                                                       connectionString,
                                                       customDatabase,
                                                       collectionPrefix,
                                                       optKey,
                                                       configConnectionString,
                                                       buildRepository);

            return this;
        }

        /// <summary>
        /// Get a storage dedicated to specific key.
        /// </summary>
        public IMARIMongoStorageBuilder SetupGrainStateStorage(IReadOnlyCollection<string> keys,
                                                                    string? connectionString = null,
                                                                    string? customDatabase = null,
                                                                    string? collectionPrefix = null,
                                                                    bool buildRepository = false)
        {
            foreach (var key in keys)
                SetupGrainStateStorage(key, connectionString, customDatabase, collectionPrefix, buildRepository);

            return this;
        }

        /// <inheritdoc />
        public IMARIMongoStorageBuilder SetupRepositoryStorage(string key,
                                                                    string? connectionString = null,
                                                                    string? customDatabase = null,
                                                                    string? collectionPrefix = null)
        {
            var optKey = ConfigurationNodeSectionNames.NodeRepositoryStorages +
                         ConfigurationSectionNames.SectionSeparator +
                         key;

            var configConnectionString = ConfigurationNodeSectionNames.NodeRepositoryStorages +
                                         ConfigurationSectionNames.SectionSeparator +
                                         key +
                                         ConfigurationSectionNames.SectionSeparator +
                                         ConfigurationSectionNames.ConnectionString;

            SetupRepositoryStorageImpl<MongoDBOptions>(key,
                                                       connectionString,
                                                       customDatabase,
                                                       collectionPrefix,
                                                       optKey,
                                                       configConnectionString);
            return this;
        }

        /// <inheritdoc />
        public IMARIMongoStorageBuilder SetupRepositoryStorage(IReadOnlyCollection<string> keys,
                                                                    string? connectionString = null,
                                                                    string? customDatabase = null,
                                                                    string? collectionPrefix = null)
        {
            foreach (var key in keys)
                SetupRepositoryStorage(key, connectionString, customDatabase, collectionPrefix);

            return this;
        }

        /// <inheritdoc />
        public IMARIMongoStorageBuilder SetupDefinitionStorage(string key, string? connectionString = null, string? customDatabase = null, string? collectionPrefix = null)
        {
            var optKey = ConfigurationNodeSectionNames.NodeDefinitionProvider +
                         ConfigurationSectionNames.SectionSeparator +
                         key;

            var configConnectionString = ConfigurationNodeSectionNames.NodeDefinitionProvider +
                                         ConfigurationSectionNames.SectionSeparator +
                                         key +
                                         ConfigurationSectionNames.SectionSeparator +
                                         ConfigurationSectionNames.ConnectionString;

            var option = BuildConnectionOption<MongoDBDefinitionConnectionOptions>(key,
                                                                                   connectionString,
                                                                                   customDatabase,
                                                                                   collectionPrefix,
                                                                                   optKey,
                                                                                   ConfigurationNodeSectionNames.NodeReminderStateMemoryConnectionString,
                                                                                   out var connectionOption);

            var siloBuilder = this._mariBaseGenericBuilder.SourceOrleanBuilder as ISiloBuilder;

#pragma warning disable IDE0270 // Use coalesce expression
            if (siloBuilder == null)
                throw new InvalidOperationException("The auto configurator must only be used by Node/Server side");
#pragma warning restore IDE0270 // Use coalesce expression

            var services = siloBuilder.Services;

            SetupOptions(key, option, connectionOption, services);

            services.TryAddSingleton<CacheProxyRepositorySpecificFactory<MongoRepositoryDefinitionFactory>>();
            services.AddKeyedSingleton<IRepositorySpecificFactory>(key, (p, k) => p.GetRequiredService<CacheProxyRepositorySpecificFactory<MongoRepositoryDefinitionFactory>>());

            return this;
        }

        /// <summary>
        /// Adds a bson convert.
        /// </summary>
        public IMARIMongoStorageBuilder AddConvert<TTarget>(IBsonSerializer<TTarget> bsonSerializer)
        {
            BsonSerializer.RegisterSerializer<TTarget>(bsonSerializer);
            return this;
        }

        /// <inheritdoc />
        public void SetupMongoCluster(IMARIClusterBuilder cl,
                                      string? connectionString,
                                      MongoDBMembershipTableOptions? option,
                                      string? serviceId,
                                      string? clusterId)
        {
            MongoDBOptions? genericOpt = null;
            MongoDBConnectionOptions? connectionOption = null;

            if (cl.IsServerNode)
            {
                cl.AddMembershipTable<MongoMembershipTable>();
                cl.AddConfigurationValidator<MongoDBOptionsValidator<MongoDBMembershipTableOptions>>();

                var opt = BuildConnectionOption<MongoDBMembershipTableOptions>(string.Empty,
                                                                               connectionString,
                                                                               option?.DatabaseName,
                                                                               option?.CollectionPrefix,
                                                                               ConfigurationSectionNames.ClusterMembership,
                                                                               ConfigurationSectionNames.ClusterMembershipConnectionStringKey,
                                                                               out connectionOption);
                SetupOptions(MongoDBConnectionOptions.MARI_CLUSTER, opt, connectionOption, cl.GetServiceCollection());

                genericOpt = opt;
                cl.AddExtensionOption(opt);
            }
            else
            {
                cl.AddGatewayListProvider<MongoGatewayListProvider>();
                cl.AddConfigurationValidator<MongoDBOptionsValidator<MongoDBGatewayListProviderOptions>>();

                var opt = BuildConnectionOption<MongoDBGatewayListProviderOptions>(string.Empty,
                                                                                   connectionString,
                                                                                   option?.DatabaseName,
                                                                                   option?.CollectionPrefix,
                                                                                   ConfigurationSectionNames.ClusterMembership,
                                                                                   ConfigurationSectionNames.ClusterMembershipConnectionStringKey,
                                                                                   out connectionOption);

                SetupOptions(MongoDBConnectionOptions.MARI_CLUSTER, opt, connectionOption, cl.GetServiceCollection());

                //var clientCopy = new MongoDBGatewayListProviderOptions()
                //{
                //    ClientName = opt.ClientName,
                //    CollectionConfigurator = opt.CollectionConfigurator,
                //    CollectionPrefix = opt.CollectionPrefix,
                //    CreateShardKeyForCosmos = opt.CreateShardKeyForCosmos,
                //    DatabaseName = opt.DatabaseName,
                //    Strategy = ((MongoDBGatewayListProviderOptions)opt).Strategy
                //};

                genericOpt = opt;
                cl.AddExtensionOption(opt);
            }

            var services = cl.GetServiceCollection();

            if (connectionOption is not null)
            {
                connectionOption.Key = MongoDBConnectionOptions.MARI_CLUSTER;
                services.AddSingleton(connectionOption);
            }

            //this._serviceCollection.PostConfigure<MongoDBMembershipTableOptions>(m => MongoConfigurator.MapOption(opt, m ?? new MongoDBMembershipTableOptions()));
            //this._serviceCollection.PostConfigure<MongoDBGatewayListProviderOptions>(m => MongoConfigurator.MapOption(opt, m ?? new MongoDBGatewayListProviderOptions()));

            cl.CustomizeClusterId(serviceId ?? ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, clusterId ?? ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);
        }

        #region Tools

        /// <summary>
        /// Get a storage dedicated to specific key.
        /// </summary>
        private IMARIMongoStorageBuilder SetupGrainStateStorageImpl<TOption>(string key,
                                                                                  string? connectionString,
                                                                                  string? customDatabase,
                                                                                  string? collectionPrefix,
                                                                                  string optionKey,
                                                                                  string connectionStringConfigKey,
                                                                                  bool buildRepository)
             where TOption : MongoDBOptions, new()
        {
            var option = BuildConnectionOption<TOption>(key,
                                                        connectionString,
                                                        customDatabase,
                                                        collectionPrefix ?? "Grains",
                                                        optionKey,
                                                        connectionStringConfigKey,
                                                        out var connectionOption);

            var siloBuilder = this._mariBaseGenericBuilder.SourceOrleanBuilder as ISiloBuilder;

#pragma warning disable IDE0270 // Use coalesce expression
            if (siloBuilder == null)
                throw new InvalidOperationException("The auto configurator must only be used by Node/Server side");
#pragma warning restore IDE0270 // Use coalesce expression

            var services = siloBuilder.Services;

            SetupOptions(key, option, connectionOption, services);

            siloBuilder.AddMongoDBGrainStorage(key, o => MongoConfigurator.MapOption(option, o));

            if (buildRepository)
            {
                services.RemoveKeyedService<string, IRepositorySpecificFactory>(key);
                services.TryAddSingleton<CacheProxyRepositorySpecificFactory<MongoRepositoryGrainStateFactory>>();
                services.AddKeyedSingleton<IRepositorySpecificFactory>(key, (p, k) => p.GetRequiredService<CacheProxyRepositorySpecificFactory<MongoRepositoryGrainStateFactory>>());
            }

            return this;
        }

        /// <summary>
        /// Get a storage dedicated to specific key.
        /// </summary>
        private IMARIMongoStorageBuilder SetupRepositoryStorageImpl<TOption>(string key,
                                                                                  string? connectionString,
                                                                                  string? customDatabase,
                                                                                  string? collectionPrefix,
                                                                                  string optionKey,
                                                                                  string connectionStringConfigKey)
             where TOption : MongoDBOptions, new()
        {
            var option = BuildConnectionOption<TOption>(key,
                                                        connectionString,
                                                        customDatabase,
                                                        collectionPrefix,
                                                        optionKey,
                                                        connectionStringConfigKey,
                                                        out var connectionOption);

            var siloBuilder = this._mariBaseGenericBuilder.SourceOrleanBuilder as ISiloBuilder;

#pragma warning disable IDE0270 // Use coalesce expression
            if (siloBuilder == null)
                throw new InvalidOperationException("The auto configurator must only be used by Node/Server side");
#pragma warning restore IDE0270 // Use coalesce expression

            option ??= new TOption();

            var services = siloBuilder.Services;

            SetupOptions(key, option, connectionOption, services);

            services.TryAddSingleton<CacheProxyRepositorySpecificFactory<MongoRepositoryFactory>>();
            services.AddKeyedSingleton<IRepositorySpecificFactory>(key, (p, k) => p.GetRequiredService<CacheProxyRepositorySpecificFactory<MongoRepositoryFactory>>());

            return this;
        }

        /// <summary>
        /// Setups the options.
        /// </summary>
        private static void SetupOptions<TOption>(string key,
                                                  TOption option,
                                                  MongoDBConnectionOptions connectionOption,
                                                  IServiceCollection services) where TOption : MongoDBOptions, new()
        {
            services.AddKeyedSingleton(key, option.ToOption());
            services.AddKeyedSingleton(key, option.ToMonitorOption());

            services.AddKeyedSingleton(key, option.ToOption<MongoDBOptions>());
            services.AddKeyedSingleton(key, option.ToMonitorOption<MongoDBOptions>());

            services.AddKeyedSingleton(key, connectionOption.ToOption());
            services.AddKeyedSingleton(key, connectionOption.ToMonitorOption());
        }

        /// <summary>
        /// Generates the options.
        /// </summary>
        private TOptionType BuildConnectionOption<TOptionType>(string key,
                                                               string? connectionString,
                                                               string? customDatabase,
                                                               string? collectionPrefix,
                                                               string configurationKey,
                                                               string connectionStringConfigurationKey,
                                                               out MongoDBConnectionOptions connectionOptions)
            where TOptionType : MongoDBOptions, new()
        {
            connectionOptions = new MongoDBConnectionOptions();

            TOptionType? optMapped = null;

            if (!string.IsNullOrEmpty(configurationKey))
                optMapped = this._configuration.GetSection(configurationKey).Get<TOptionType>();

            connectionOptions.Key = key;
            connectionOptions.ConnectionString = connectionString;
            connectionOptions.DatabaseName = customDatabase ?? optMapped?.DatabaseName;
            connectionOptions.CollectionPrefix = collectionPrefix ?? optMapped?.CollectionPrefix;
            connectionOptions.Order = this._connectionInfoOrder++;

            if (optMapped is not null)
                connectionOptions.CreateShardKeyForCosmos = optMapped.CreateShardKeyForCosmos;
            else
                connectionOptions.CreateShardKeyForCosmos = this._lastConnectionConfiguration.CreateShardKeyForCosmos;

            if (string.IsNullOrEmpty(connectionOptions.ConnectionString) && !string.IsNullOrEmpty(connectionStringConfigurationKey))
                connectionOptions.ConnectionString = this._configuration.GetSection(connectionStringConfigurationKey).Get<string>();

            if (string.IsNullOrEmpty(connectionOptions.DatabaseName))
                connectionOptions.DatabaseName = this._lastConnectionConfiguration.DatabaseName;

            if (string.IsNullOrEmpty(connectionOptions.ConnectionString))
                connectionOptions.ConnectionString = this._lastConnectionConfiguration.ConnectionString;

            var mappedOpt = MongoConfigurator.MapOption(connectionOptions, optMapped ?? new TOptionType());

            this._lastConnectionConfiguration = connectionOptions;

            return mappedOpt;
        }

        #endregion

        #endregion
    }
}
