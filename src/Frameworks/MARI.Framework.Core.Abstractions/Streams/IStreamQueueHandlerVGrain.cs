// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.StreamQueue
{
    /// <summary>
    /// Define an handler used to keep data provider through the stream connected
    /// </summary>
    public interface IStreamQueueHandlerVGrain : IVGrain, IGrainWithGuidKey
    {
        /// <summary>
        /// Update stream status and ensure information correspond to definitions
        /// </summary>
        Task UpdateAsync();
    }
}