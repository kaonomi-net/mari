// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions
{
    using Orleans.Runtime;
    using Orleans.Services;

    /// <summary>
    /// Provider of <see cref="IComponentIdentityCard"/>
    /// </summary>
    internal interface IComponentIdentitCardProvider : IGrainService
    {
        /// <summary>
        /// Gets the component identity card from <paramref name="componentId"/>
        /// </summary>
        ValueTask<IComponentIdentityCard> GetComponentIdentityCardAsync(GrainId grainId, Guid componentId);
    }
}
