// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// Keep : MARI.Framework.Configurations; to easy configuration use
namespace MARI.Framework.Configurations
{
    using MARI.Framework.Cluster.Configurations.Builders;
    using MARI.Framework.Core.Abstractions.Enums;
    using MARI.Framework.Extensions.Mongo.Configurations;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    using Orleans.Providers;
    using Orleans.Providers.MongoDB.Configuration;

    using System;

    /// <summary>
    /// Extensions use to setup AdoNet memory cluster from <see cref="IClusterNodeBuilderMARIWizardStart"/>
    /// </summary>

    public static class MARIMongoBuilderMARIWizardStartExtensions
    {
        #region Fields

        private static readonly Type s_builderTraits = typeof(IMARIMongoStorageBuilder);

        #endregion

        #region Methods

        /// <summary>
        /// Setup Mongo db cluster synchronication
        /// </summary>
        public static TWizard UseMongoCluster<TWizard, TWizardConfig>(this IMARIWizardStart<TWizard, TWizardConfig> wizard,
                                                                      Action<IMARIClusterExternalBuilder<MongoDBMembershipTableOptions>>? buildOption = null,
                                                                      string? serviceId = null,
                                                                      string? clusterId = null)
            where TWizard : IMARIWizard<TWizard, TWizardConfig>
            where TWizardConfig : IMARICoreConfigurationWizard<TWizardConfig>
        {
            return wizard.SetupCluster((cl, cfg) =>
            {
                var builder = new MARIExternalClusterBuilder<MongoDBMembershipTableOptions>(cfg);

                buildOption?.Invoke(builder);

                var buildResult = builder.Build();

                var mongoBuilder = GetBuilder(cl.GetServiceCollection(), cl.GetConfiguration(), cl);
                mongoBuilder.SetupMongoCluster(cl, buildResult.ConnectionString, buildResult.Option, serviceId, clusterId);
            });
        }

        /// <summary>
        /// Setup Mongo DB to storage
        /// </summary>
        /// <param name="storageType">Define whitch type of storage impacted</param>
        /// <param name="connectionString">Set the connection if different or not set during the cluster setups</param>
        public static IMARINodeMemoryBuilder UseMongo(this IMARINodeMemoryBuilder wizard,
                                                           StorageTypeEnum storageType = StorageTypeEnum.All,
                                                           string? connectionString = null,
                                                           string? database = null,
                                                           string? collectionPrefix = null)
        {
            var mongoBuilder = GetBuilder(wizard.GetServiceCollection(), wizard.GetConfiguration(), wizard);

            mongoBuilder.Store(storageType, connectionString, database, collectionPrefix);
            return wizard;
        }

        /// <summary>
        /// Setup Mongo DB to storage
        /// </summary>
        /// <param name="storageType">Define whitch type of storage impacted</param>
        /// <param name="connectionString">Set the connection if different or not set during the cluster setups</param>
        public static IMARINodeMemoryBuilder UseMongo(this IMARINodeMemoryBuilder wizard,
                                                           Action<IMARIMongoStorageBuilder> builderAction)
        {
            var mongoBuilder = GetBuilder(wizard.GetServiceCollection(), wizard.GetConfiguration(), wizard);
            builderAction(mongoBuilder);
            return wizard;
        }

        /// <summary>
        /// Setup Mongo DB to storage
        /// </summary>
        /// <param name="storageType">Define whitch type of storage impacted</param>
        /// <param name="connectionString">Set the connection if different or not set during the cluster setups</param>
        public static IMARINodeMemoryBuilder UseMongoGrainStorage(this IMARINodeMemoryBuilder wizard,
                                                                       string key,
                                                                       string? connectionString = null,
                                                                       string? database = null,
                                                                       string? collectionPrefix = null,
                                                                       bool buildRepository = false)
        {
            var builder = GetBuilder(wizard.GetServiceCollection(), wizard.GetConfiguration(), wizard);
            builder.SetupGrainStateStorage(key,
                                           connectionString,
                                           database,
                                           collectionPrefix,
                                           buildRepository);

            return wizard;
        }

        /// <summary>
        /// Setup Mongo DB to storage
        /// </summary>
        /// <param name="storageType">Define whitch type of storage impacted</param>
        /// <param name="connectionString">Set the connection if different or not set during the cluster setups</param>
        public static IMARINodeMemoryBuilder UseMongoGrainStorage(this IMARINodeMemoryBuilder wizard,
                                                                       IReadOnlyCollection<string> keys,
                                                                       string? connectionString = null,
                                                                       string? database = null,
                                                                       string? collectionPrefix = null,
                                                                       bool buildRepository = false)
        {
            var builder = GetBuilder(wizard.GetServiceCollection(), wizard.GetConfiguration(), wizard);
            builder.SetupGrainStateStorage(keys,
                                           connectionString,
                                           database,
                                           collectionPrefix,
                                           buildRepository);

            return wizard;
        }

        /// <summary>
        /// Setup Mongo DB to storage as repository
        /// </summary>
        /// <param name="storageType">Define whitch type of storage impacted</param>
        /// <param name="connectionString">Set the connection if different or not set during the cluster setups</param>
        public static IMARINodeMemoryBuilder UseMongoRepository(this IMARINodeMemoryBuilder wizard,
                                                                     string key,
                                                                     string? connectionString = null,
                                                                     string? database = null,
                                                                     string? collectionPrefix = null)
        {
            var builder = GetBuilder(wizard.GetServiceCollection(), wizard.GetConfiguration(), wizard);
            builder.SetupRepositoryStorage(key,
                                           connectionString,
                                           database,
                                           collectionPrefix);

            return wizard;
        }

        /// <summary>
        /// Setup Mongo DB to storage as repository
        /// </summary>
        /// <param name="storageType">Define whitch type of storage impacted</param>
        /// <param name="connectionString">Set the connection if different or not set during the cluster setups</param>
        public static IMARINodeMemoryBuilder UseMongoRepository(this IMARINodeMemoryBuilder wizard,
                                                                 IReadOnlyCollection<string> keys,
                                                                 string? connectionString = null,
                                                                 string? database = null,
                                                                 string? collectionPrefix = null)
        {
            var builder = GetBuilder(wizard.GetServiceCollection(), wizard.GetConfiguration(), wizard);
            builder.SetupRepositoryStorage(keys,
                                           connectionString,
                                           database,
                                           collectionPrefix);

            return wizard;
        }

        /// <summary>
        /// Adds the mongo as a definition provider.
        /// </summary>
        public static IMARINodeWizard AddMongoDefinitionProvider(this IMARINodeWizard wizard,
                                                                      Action<IMARIClusterExternalBuilder<MongoDBOptions>> buildOption,
                                                                      string uniqueKeyName = ProviderConstants.DEFAULT_LOG_CONSISTENCY_PROVIDER_NAME)
        {
            //wizard.SetupNodeMemories(memory =>
            //{
            //    var builder = new MARIExternalClusterBuilder<MongoDBOptions>(memory.GetConfiguration());
            //    buildOption?.Invoke(builder);

            //    var buildResult = builder.Build();

            //    AutoCustomDefinitionProviderMongoConfigurator.Default
            //                                                 .ConfigureMongoProviderStorage(memory,
            //                                                                                memory.GetConfiguration(),
            //                                                                                memory.GetServiceCollection(),
            //                                                                                memory.Logger,
            //                                                                                buildResult.ConnectionString,
            //                                                                                buildResult.Option,
            //                                                                                uniqueKeyName);
            //});

            //return wizard;

            throw new NotImplementedException();
        }

        #region Tools

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public static IMARIMongoStorageBuilder GetBuilder(IServiceCollection services, IConfiguration configuration, IMARIBaseGenericBuilder mariBaseGenericBuilder)
        {
            var builder = services.FirstOrDefault(s => s.IsKeyedService == false &&
                                                       s.ServiceType == s_builderTraits &&
                                                       s.ImplementationInstance is not null)?.ImplementationInstance as IMARIMongoStorageBuilder;

            if (builder == null)
            {
                builder = new MARIMongoStorageBuilder(services, configuration, mariBaseGenericBuilder);
                services.AddSingleton<IMARIMongoStorageBuilder>(builder);
            }

            return builder;
        }

        #endregion

        #endregion
    }
}
