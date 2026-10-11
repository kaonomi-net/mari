// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.StreamQueue
{
    using MARI.Framework.Core.Abstractions.Attributes;

    using Orleans.Services;

    /// <summary>
    /// Grain service in charge to handled trigger link to a stream
    /// </summary>
    /// <seealso cref="IGrainService" />
    [MARISystemVGrain]
    public interface IStreamTriggerVGrainService : IGrainService
    {
    }
}
