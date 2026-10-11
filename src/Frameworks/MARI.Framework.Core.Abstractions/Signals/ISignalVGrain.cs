// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Signals
{
    using MARI.Framework.Core.Abstractions.Attributes;
    using MARI.Framework.Core.Abstractions.Models;

    using Orleans.Concurrency;
    using Orleans.Runtime;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Define the virtual grain in charge of a specific signal
    /// </summary>
    /// <seealso cref="IGrainWithGuidKey" />
    [MARISystemVGrain]
    internal interface ISignalVGrain : IGrainWithGuidKey, ISignalHandler
    {
        /// <summary>
        /// Fires the signal configured
        /// </summary>
        [OneWay]
        [ReadOnly]
        Task<Guid> Fire(Guid fireId, GrainId? sourceId, VGrainMetaData? sourceMetaData, GrainCancellationToken token);

        /// <summary>
        /// Fires the signal configured
        /// </summary>
        [OneWay]
        [ReadOnly]
        Task<Guid> Fire<TData>(Guid fireId, GrainId? sourceId, TData data, VGrainMetaData? sourceMetaData, GrainCancellationToken token)
            where TData : struct;
    }
}
