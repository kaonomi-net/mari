// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Cron
{
    using MARI.Framework.Core.Abstractions.Attributes;
    using MARI.Framework.Node.Abstractions.Triggers;

    /// <summary>
    /// Handler in charge to manager trigger using con expression as source
    /// </summary>
    /// <remarks>
    ///     By default the grain id match the trigger definition.
    /// </remarks>
    [MARISystemVGrain]
    public interface ICronTriggerHandlerVGrain : ITriggerHandlerVGrain, IGrainWithGuidKey
    {

    }
}
