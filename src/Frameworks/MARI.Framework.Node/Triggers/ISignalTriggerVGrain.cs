// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Triggers
{
    using MARI.Framework.Core.Abstractions.Attributes;
    using MARI.Framework.Core.Abstractions.Signals;
    using MARI.Framework.Node.Abstractions.Triggers;

    /// <summary>
    /// Virtual grain in charge to managed one trigger
    /// </summary>
    [MARISystemVGrain]
    internal interface ISignalTriggerVGrain : ITriggerHandlerVGrain, IGrainWithGuidKey, ISignalReceiver, ISignalReceiverReadOnly
    {
    }
}
