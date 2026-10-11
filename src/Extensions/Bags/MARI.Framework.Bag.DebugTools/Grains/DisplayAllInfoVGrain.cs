// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Bag.DebugTools.Grains
{
    using MARI.Framework.Bag.DebugTools;
    using MARI.Framework.Bag.DebugTools.Models;
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Orleans.Concurrency;
    using Orleans.Placement;

    using System.Threading.Tasks;

    /// <summary>
    /// Grain used to display on the logger input and context info
    /// </summary>
    /// <seealso cref="VGrainBase{IDisplayInfoVGrain}" />
    /// <seealso cref="IDisplayInfoVGrain" />
    [Reentrant]
    [StatelessWorker]
    [PreferLocalPlacement]
    internal sealed class DisplayAllInfoVGrain : DisplayAllInfoBaseVGrain<IDisplayInfoVGrain>, IDisplayInfoVGrain
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayAllInfoVGrain"/> class.
        /// </summary>
        /// <param name="logger"></param>
        public DisplayAllInfoVGrain(ILogger<IDisplayInfoVGrain> logger,
                                    IOptionsMonitor<DebugDisplayInfoOptions>? options = null)
            : base(logger, options)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        [OneWay]
        [ReadOnly]
        public Task DisplayCallInfoAsync(IExecutionContext ctx)
        {
            return DisplayInfoAsync(ctx, nameof(IExecutionContext)).AsTask();
        }

        /// <inheritdoc />
        [ReadOnly]
        public async Task<TInput> DisplayCallInfoAsync<TInput>(TInput input, IExecutionContext ctx)
        {
            await DisplayInfoAsync(ctx, nameof(IExecutionContext), input, nameof(input));
            return input;
        }

        /// <inheritdoc />
        [ReadOnly]
        public async Task<TInput> DisplayCallInfoAsync<TInput, TConfig>(TInput input, IExecutionContext<TConfig> ctx)
        {
            await DisplayInfoAsync(ctx, nameof(IExecutionContext), input, nameof(input));
            return input;
        }

        #endregion
    }
}
