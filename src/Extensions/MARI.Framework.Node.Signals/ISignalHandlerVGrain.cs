// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Signals
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Attributes;
    using MARI.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Virtual Grain in charge of one signal fire and subsciption
    /// </summary>
    /// <seealso cref="IGrainWithGuidKey" />
    [MARISystemVGrain]
    internal interface ISignalHandlerVGrain : ISignalVGrain, IGrainWithGuidKey, IVGrain
    {
        /// <summary>
        /// Initializes the signal handler
        /// </summary>
        ValueTask InitializeAsync(GrainCancellationToken token);
    }
}
