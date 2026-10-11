// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.References
{
    using MARI.Framework.Core.Abstractions.Models;

    using Orleans.Runtime;
    using Orleans.Runtime.Services;
    using Orleans.Services;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Service in charge to solve the <see cref="Uri"/> RefId
    /// </summary>
    internal sealed class MARITypeReferenceGrainServiceClient : GrainServiceClient<IMARITypeReferenceGrainService>, IGrainServiceClient<IMARITypeReferenceGrainService>, IMARITypeReferenceGrainServiceClient
    {
        #region Fields
        
        private readonly IMARITypeReferenceGrainService _currentGrainService;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MARITypeReferenceGrainServiceClient"/> class.
        /// </summary>
        public MARITypeReferenceGrainServiceClient(IServiceProvider serviceProvider,
                                                        Silo silo)
            : base(serviceProvider)
        {
            this._currentGrainService = base.GetGrainService(silo.SiloAddress);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask<ReferenceTargetRegistry?> GetLatestRegistryAsync(string myCurrentEtag, GrainCancellationToken token)
        {
            return this._currentGrainService.GetLatestRegistryAsync(myCurrentEtag, token);
        }

        /// <inheritdoc />
        public ValueTask<bool> IsToUpdate(string currentRegistryEtag)
        {
            return this._currentGrainService.IsToUpdate(currentRegistryEtag);
        }

        #endregion
    }
}
