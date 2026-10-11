// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Cron
{
    using MARI.Framework.Core.Abstractions.Attributes;

    using Orleans.Services;

    /// <summary>
    /// Virtual Grain Service in charge to managed all cron grain instance.
    /// </summary>
    [MARISystemVGrain]
    public interface ICronVGrainService : IGrainService
    {

    }
}
