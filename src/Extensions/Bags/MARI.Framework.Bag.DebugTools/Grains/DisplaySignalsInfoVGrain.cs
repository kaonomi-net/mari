// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Bag.DebugTools.Grains
{
    using MARI.Framework.Bag.DebugTools;
    using MARI.Framework.Bag.DebugTools.Models;
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Signals;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Orleans.Concurrency;
    using Orleans.Placement;

    using System.Threading.Tasks;

    /// <summary>
    /// Grain used to display on the logger <see cref="SignalMessage"/>
    /// </summary>
    /// <seealso cref="VGrainBase{IDisplaySignalsInfoVGrain}" />
    /// <seealso cref="IDisplaySignalsInfoVGrain" />
    [Reentrant]
    [StatelessWorker]
    [PreferLocalPlacement]
    internal sealed class DisplaySignalsInfoVGrain : DisplayAllInfoBaseVGrain<IDisplaySignalsInfoVGrain>, IDisplaySignalsInfoVGrain
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplaySignalsInfoVGrain"/> class.
        /// </summary>
        public DisplaySignalsInfoVGrain(ILogger<IDisplaySignalsInfoVGrain> logger,
                                    IOptionsMonitor<DebugDisplayInfoOptions>? options = null)
            : base(logger, options)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        [OneWay]
        [ReadOnly]
        public Task DisplaySignalInfoAsync(SignalMessage signal, IExecutionContext ctx)
        {
            return DisplayInfoAsync(ctx, nameof(IExecutionContext), signal, nameof(signal)).AsTask();
        }

        /// <inheritdoc />
        [OneWay]
        [ReadOnly]
        public async Task ReceiveSignalAsync(SignalMessage signal)
        {
            await DisplayInfoAsync(signal, nameof(signal));
        }

        #endregion
    }
}
