// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Configurations
{
    using MARI.Framework.Configurations;
    using MARI.Framework.Core.Abstractions.Enums;
    using MARI.Framework.Node.Models;

    using Microsoft.Extensions.DependencyInjection;

    using Orleans.Configuration;

    public static class MARIMemoryInLocalConfiguration
    {
        /// <summary>
        /// Configures the memory storage options
        /// </summary>
        public static IMARINodeMemoryBuilder ConfigureMemoryStorage(this IMARINodeMemoryBuilder wizard, uint numStorageGrains = 10, uint initStage = MemoryGrainStorageOptions.DEFAULT_INIT_STAGE)
        {
            // MemoryGrainStorageOptions
            wizard.GetServiceCollection().AddSingleton(new DefaultMemoryGrainStorageOptions(numStorageGrains, initStage));
            return wizard;
        }

        /// <summary>
        /// Gets the in memory storage builder
        /// </summary>
        internal static IMARIMemoryInLocalBuilder GetBuilder(IMARINodeMemoryBuilder memoryBuilder)
        {
            var services = memoryBuilder.GetServiceCollection();

            var inst = services.FirstOrDefault(s => s.IsKeyedService == false &&
                                                    s.ServiceType == typeof(IMARIMemoryInLocalBuilder) &&
                                                    s.ImplementationInstance is IMARIMemoryInLocalBuilder)?.ImplementationInstance as IMARIMemoryInLocalBuilder;

            if (inst is null)
            {
                inst = new MARIMemoryInLocalBuilder(memoryBuilder);
                services.AddSingleton<IMARIMemoryInLocalBuilder>(inst);
            }

            return inst;
        }

        /// <summary>
        /// Configure system storage in memory
        /// </summary>
        public static IMARINodeMemoryBuilder UseInMemory(this IMARINodeMemoryBuilder wizard, StorageTypeEnum storageType = StorageTypeEnum.All)
        {
            var builder = GetBuilder(wizard);
            builder.Store(storageType);

            return wizard;
        }

        /// <summary>
        /// Configure grain system storage in memory
        /// </summary>
        /// <param name="buildReadOnlyRepository">If true create a repository to look up data throught cluster memory.
        ///  Attention : Memory storage on the cluster is efficient to access using _stateName and StorageName keys otherwise we have to get all the information
        ///     to filtered it is not efficient to use with knowledge of performance issue.</param>
        public static IMARINodeMemoryBuilder UseInMemoryGrainStorage(this IMARINodeMemoryBuilder wizard, string? configurationKey = null, bool buildReadOnlyRepository = false)
        {
            var builder = GetBuilder(wizard);

            configurationKey = string.IsNullOrEmpty(configurationKey) ? MARIConstants.DefaultMARIStateConfigurationKey : configurationKey;
            builder.SetupGrainStateStorage(configurationKey, buildReadOnlyRepository);

            return wizard;
        }

        /// <summary>
        /// Configure repository system storage in memory
        /// </summary>
        /// <param name="buildRepository">If true create a repository to look up data throught cluster memory.
        ///  Attention : Memory storage on the cluster is efficient to access using _stateName and StorageName keys otherwise we have to get all the information
        ///     to filtered it is not efficient to use with knowledge of performance issue.</param>
        public static IMARINodeMemoryBuilder UseInMemoryRepository(this IMARINodeMemoryBuilder wizard, string? configurationKey = null)//, bool allowWrite = true)
        {
            var builder = GetBuilder(wizard);

            configurationKey = string.IsNullOrEmpty(configurationKey) ? MARIConstants.DefaultMARIRepositoryConfigurationKey : configurationKey;
            builder.SetupRepositoryStorage(configurationKey);

            return wizard;
        }
    }
}
