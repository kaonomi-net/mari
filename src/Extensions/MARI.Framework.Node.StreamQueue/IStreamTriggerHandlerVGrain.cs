// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.StreamQueue
{
    using MARI.Framework.Node.Abstractions.Triggers;

    /// <summary>
    /// Handler of trigger based on stream information
    /// </summary>
    /// <seealso cref="ITriggerHandlerVGrain" />
    /// <seealso cref="IGrainWithGuidCompoundKey" />
    public interface IStreamTriggerHandlerVGrain : ITriggerHandlerVGrain, IGrainWithGuidCompoundKey
    {
    }
}
