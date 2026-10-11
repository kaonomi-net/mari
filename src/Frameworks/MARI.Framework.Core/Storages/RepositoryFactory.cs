// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Storages
{
    using MARI.Framework.Core.Abstractions.Models;
    using MARI.Framework.Core.Abstractions.Repositories;
    using MARI.Framework.Core.Abstractions.Storages;
    using MARI.Framework.Core.Repositories;

    using MARI.Toolbox.Abstractions.Supports;
    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using Orleans.Providers;

    using System;

    /// <summary>
    /// Global repository factory
    /// </summary>
    /// <seealso cref="IRepositoryFactory" />
    internal sealed class RepositoryFactory : IRepositoryFactory
    {
        #region Fields

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<IRepositoryFactory> _logger;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryFactory"/> class.
        /// </summary>
        public RepositoryFactory(IServiceProvider serviceProvider,
                                 ILogger<IRepositoryFactory>? logger = null)
        {
            this._serviceProvider = serviceProvider;
            this._logger = logger ?? NullLogger<IRepositoryFactory>.Instance;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IReadOnlyRepository<TEntity, TEntityId> Get<TTargetRepo, TEntity, TEntityId>(RepositoryGetOptions request,
                                                                                            CancellationToken token = default)
            where TTargetRepo : IReadOnlyRepository<TEntity, TEntityId>
            where TEntity : IEntityWithId<TEntityId>
            where TEntityId : notnull, IEquatable<TEntityId>
        {
            try
            {
                var configurationName = string.IsNullOrEmpty(request.ConfigurationName)
                                            ? MARIConstants.DefaultMARIRepositoryConfigurationKey
                                            : request.ConfigurationName;

                var storageName = request.StorageName ?? ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME;

                var specificFactory = this._serviceProvider.GetKeyedService<IRepositorySpecificFactory>(configurationName)
                                      ?? this._serviceProvider.GetKeyedService<IRepositorySpecificFactory>(MARIConstants.DefaultMARIRepositoryConfigurationKey)
                                      ?? this._serviceProvider.GetKeyedService<IRepositorySpecificFactory>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);

                if (specificFactory is null)
                {
                    // TODO : Get Default if null
                    throw new Exception("No specific repository factory");
                }

                var newRequest = new RepositoryGetOptions(storageName, request.IsReadOnly, configurationName, request.PreventAnyKindOfDiscriminatorUsage);

                IReadOnlyRepository<TEntity, TEntityId> repo;
                if (typeof(TTargetRepo) != typeof(IReadOnlyRepository<TEntity, TEntityId>) && typeof(TTargetRepo) != typeof(IRepository<TEntity, TEntityId>))
                    repo = specificFactory.Get<TTargetRepo, TEntity, TEntityId>(this._serviceProvider, newRequest);
                else if (request.IsReadOnly)
                    repo = new ReadOnlyRepository<TEntity, TEntityId>(specificFactory, this._serviceProvider, newRequest);
                else
                    repo = new Repository<TEntity, TEntityId>(specificFactory, this._serviceProvider, newRequest);

                if (repo is null)
                    throw new KeyNotFoundException($"Could build a dedicated repository with following pair storageName:'{storageName}' configurationName:'{configurationName}'");

                return repo;
            }
            catch (Exception ex)
            {
                this._logger.OptiLog(LogLevel.Error,
                                     "[Repository Factory] - Storage Name '{storageName}', Configuration Name '{configurationName}' - {exception}",
                                     request.StorageName,
                                     request.ConfigurationName,
                                     ex);
                throw;
            }
        }

        #endregion
    }
}