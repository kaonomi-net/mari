// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions
{
    using Orleans.Runtime;
    using Orleans.Services;

    /// <summary>
    /// Factory able to produce proxy point to any GrainService in the cluster based on unique grain Id
    /// </summary>
    public interface IRemoteGrainServiceFactory
    {
        TGrainService GetRemoteGrainService<TGrainService>(GrainId grainId) where TGrainService : IGrainService;

        TService GetRemoteGrainService<TService>(GrainId grainId, Type type);
    }
}
