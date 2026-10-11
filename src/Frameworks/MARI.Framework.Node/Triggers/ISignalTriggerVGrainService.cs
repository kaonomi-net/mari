// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Triggers
{
    using MARI.Framework.Core.Abstractions.Attributes;

    using Orleans.Services;

    /// <summary>
    /// FullKeys service use to setup and manager trigger from signal
    /// </summary>
    [MARISystemVGrain]
    internal interface ISignalTriggerVGrainService : IGrainService
    {
    }
}
