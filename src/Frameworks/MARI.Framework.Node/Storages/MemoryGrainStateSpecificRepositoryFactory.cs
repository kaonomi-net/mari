// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Storages
{
    using MARI.Framework.Core.Abstractions.Storages;
    using MARI.Framework.Node.Abstractions.Repositories;
    using MARI.Toolbox;
    using Microsoft.Extensions.DependencyInjection;

    using System;

    ///// <summary>
    ///// Factory used to produce repository based on cluster memory storage
    ///// </summary>
    ///// <seealso cref="IRepositorySpecificFactory" />
    //public sealed class MemoryGrainStateSpecificRepositoryFactory : DefaultSpecificRepositoryBaseFactory, IRepositorySpecificFactory
    //{
    //    #region Fields

    //    private static readonly Type s_monoWithEntityIdRepo;
    //    private static readonly Type s_monoRepo;

    //    #endregion

    //    #region Ctor

    //    /// <summary>
    //    /// Initializes the <see cref="MemoryRepositoryFactory"/> class.
    //    /// </summary>
    //    static MemoryGrainStateSpecificRepositoryFactory()
    //    {
    //        s_monoRepo = typeof(MemoryReadOnlyRepository<>);
    //        s_monoWithEntityIdRepo = typeof(MemoryReadOnlyRepository<,>);
    //    }

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="MemoryRepositoryFactory"/> class.
    //    /// </summary>
    //    public MemoryGrainStateSpecificRepositoryFactory(string storageName)
    //        : base(s_monoRepo, s_monoWithEntityIdRepo, NoneType.Trait, NoneType.Trait, storageName, false)
    //    {
    //    }

    //    #endregion

    //    #region Methods

    //    /// <inheritdoc />
    //    protected override object InstanciateRepository(IServiceProvider serviceProvider,
    //                                                    string stateName,
    //                                                    Type serviceTargetTrait,
    //                                                    Type repositoryType,
    //                                                    Type repositoryWithEntityIdType)
    //    {
    //        var instType = repositoryType;

    //        var genericParams = serviceTargetTrait.GetGenericArguments();

    //        if (genericParams.Length == 2)
    //            instType = repositoryWithEntityIdType;

    //        instType = instType.MakeGenericType(genericParams);

    //        return ActivatorUtilities.CreateInstance(serviceProvider, instType, stateName, this.StorageName);
    //    }

    //    #endregion
    //}
}
