// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Configurations
{
    using MARI.Framework.Configurations;
    using MARI.Framework.Core.Abstractions.Enums;
    using MARI.Framework.Core.Abstractions.Storages;
    using MARI.Framework.Core.Repositories;
    using MARI.Framework.Node.Abstractions.Repositories;
    using MARI.Framework.Node.Configurations.AutoConfigurator;
    using MARI.Framework.Node.Storages;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    using Orleans.Runtime.Hosting;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Memory storage builder
    /// </summary>
    internal sealed class MARIMemoryInLocalBuilder : IMARIMemoryInLocalBuilder
    {
        #region Fields

        private readonly IMARINodeMemoryBuilder _memoryBuilder;
        private readonly ISiloBuilder _siloBuilder;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MARIMemoryInLocalBuilder"/> class.
        /// </summary>
        public MARIMemoryInLocalBuilder(IMARINodeMemoryBuilder memoryBuilder)
        {
            this._memoryBuilder = memoryBuilder;
            this._siloBuilder = memoryBuilder.GetSiloBuilder();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IMARIMemoryInLocalBuilder SetupDefaultRepositoryStorage()
        {
            SetupRepositoryStorageImpl(MARIConstants.DefaultMARIRepositoryConfigurationKey);
            return this;
        }

        /// <inheritdoc />
        public IMARIMemoryInLocalBuilder SetupDefaultStorage()
        {
            this._siloBuilder.AddMemoryGrainStorageAsDefault(MemoryConfiguratorHelper.OptionConfigurator);
            return this;
        }

        /// <inheritdoc />
        public IMARIMemoryInLocalBuilder SetupMARIAdminStorage()
        {
            SetupGrainStateStorageImpl(MARIConstants.DefaultMARIAdminStateConfigurationKey, false);
            return this;
        }

        /// <inheritdoc />
        public IMARIMemoryInLocalBuilder SetupMARIStorage()
        {
            SetupGrainStateStorageImpl(MARIConstants.DefaultMARIStateConfigurationKey, false);
            return this;
        }

        /// <inheritdoc />
        public IMARIMemoryInLocalBuilder SetupDynamicDefinitionStorage()
        {
            SetupGrainStateStorageImpl(MARIConstants.DefaultMARIDynamicDefinitionsConfigurationKey, false);
            SetupRepositoryStorageImpl(MARIConstants.DefaultMARIDynamicDefinitionsRepositoryConfigurationKey);
            return this;
        }

        /// <inheritdoc />
        public IMARIMemoryInLocalBuilder SetupGrainStateStorage(string key, bool buildRepository = false)
        {
            SetupGrainStateStorageImpl(key, buildRepository);
            return this;
        }

        /// <inheritdoc />
        public IMARIMemoryInLocalBuilder SetupGrainStateStorage(IReadOnlyCollection<string> keys, bool buildRepository = false)
        {
            foreach (var key in keys)
                SetupGrainStateStorageImpl(key, buildRepository);

            return this;
        }

        /// <inheritdoc />
        public IMARIMemoryInLocalBuilder SetupReminderStorage()
        {
            this._siloBuilder.UseInMemoryReminderService();
            return this;
        }

        /// <inheritdoc />
        public IMARIMemoryInLocalBuilder SetupRepositoryStorage(string key)
        {
            SetupRepositoryStorageImpl(key);
            return this;
        }

        /// <inheritdoc />
        public IMARIMemoryInLocalBuilder SetupRepositoryStorage(IReadOnlyCollection<string> keys)
        {
            foreach (var key in keys)
                SetupRepositoryStorageImpl(key);

            return this;
        }

        /// <inheritdoc />
        public IMARIMemoryInLocalBuilder Store(StorageTypeEnum storageType)
        {
            if ((storageType & StorageTypeEnum.Default) == StorageTypeEnum.Default)
            {
                SetupDefaultStorage();
                storageType -= StorageTypeEnum.Default;
            }

            if ((storageType & StorageTypeEnum.Reminders) == StorageTypeEnum.Reminders)
            {
                SetupReminderStorage();
                storageType -= StorageTypeEnum.Reminders;
            }

            if ((storageType & StorageTypeEnum.MARI) == StorageTypeEnum.MARI)
            {
                SetupMARIStorage();
                storageType -= StorageTypeEnum.MARI;
            }

            if ((storageType & StorageTypeEnum.DynamicDefinition) == StorageTypeEnum.DynamicDefinition)
            {
                SetupDynamicDefinitionStorage();
                storageType -= StorageTypeEnum.DynamicDefinition;
            }

            if ((storageType & StorageTypeEnum.MARIAdmin) == StorageTypeEnum.MARIAdmin)
            {
                SetupMARIAdminStorage();
                storageType -= StorageTypeEnum.MARIAdmin;
            }

            if ((storageType & StorageTypeEnum.Repositories) == StorageTypeEnum.Repositories)
            {
                SetupDefaultRepositoryStorage();
                storageType -= StorageTypeEnum.Repositories;
            }

            if (storageType != 0)
                throw new InvalidOperationException("Storage configuration not managed : " + storageType);

            return this;
        }

        #region Tools

        /// <summary>
        /// Setups the grain state storage implementation.
        /// </summary>
        private void SetupGrainStateStorageImpl(string key, bool buildReadRepository)
        {
            this._siloBuilder.AddMemoryGrainStorage(key, MemoryConfiguratorHelper.OptionConfigurator);

            if (buildReadRepository)
            {
                this._siloBuilder.Services.AddGrainStorage(key, MemoryGrainStorageRepositoryFactory.Create);
                this._siloBuilder.Services.TryAddSingleton<CacheProxyRepositorySpecificFactory<MemorySpecificStateContainerFactory>>();
                this._siloBuilder.Services.AddKeyedSingleton<IRepositorySpecificFactory>(key, (p, k) => p.GetRequiredService<CacheProxyRepositorySpecificFactory<MemorySpecificStateContainerFactory>>());
            }
        }

        /// <summary>
        /// Setups the repository storage implementation.
        /// </summary>
        private void SetupRepositoryStorageImpl(string key)
        {
            this._siloBuilder.Services.RemoveKeyedService<string, IRepositorySpecificFactory>(key);
            this._siloBuilder.Services.TryAddSingleton<CacheProxyRepositorySpecificFactory<MemorySpecificRepositoryFactory>>();
            this._siloBuilder.Services.AddKeyedSingleton<IRepositorySpecificFactory>(key, (p, k) => p.GetRequiredService<CacheProxyRepositorySpecificFactory<MemorySpecificRepositoryFactory>>());
        }

        #endregion

        #endregion
    }
}
