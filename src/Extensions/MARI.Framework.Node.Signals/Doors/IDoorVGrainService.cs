// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Signals.Doors
{
    using MARI.Framework.Core.Abstractions.Attributes;

    using Orleans.Services;

    /// <summary>
    /// Virtual Grain Service used to managed door grain through the cluster.
    /// </summary>
    /// <seealso cref="IGrainService" />
    [MARISystemVGrain]
    internal interface IDoorVGrainService : IGrainService
    {
    }
}
