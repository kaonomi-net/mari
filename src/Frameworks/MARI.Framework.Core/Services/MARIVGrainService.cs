// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Services
{
    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Base Safe class of GRainServier
    /// </summary>
    /// <seealso cref="Orleans.Runtime.GrainService" />
    public abstract class MARIVGrainService : GrainService
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MARIVGrainService"/> class.
        /// </summary>
        protected MARIVGrainService(GrainId grainId,
                                         Silo silo,
                                         ILoggerFactory loggerFactory)
            : base(grainId, silo, loggerFactory)
        {

            this.Logger = loggerFactory.CreateLogger("[VGrain Service : " + GetType().Name + "] [Silo: '" + silo.SiloAddress + "']");
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public ILogger Logger { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public sealed override async Task Start()
        {
            try
            {
                this.Logger.OptiLog(LogLevel.Debug, "Grain Service Start ...");

                await RefreshInfoAsync();

                this.Logger.OptiLog(LogLevel.Information, "Grain Service Started");
            }
            catch (Exception ex)
            {
                // Not rethrown to keep the silo start-up behavior, but logged as critical : the service is NOT operational
                this.Logger.LogCritical(ex, "VGrain Service Start Failed, the service is not operational : {exception}", ex);
            }
        }

        /// <inheritdoc cref="GrainService.Start" />
        protected abstract Task RefreshInfoAsync();

        #endregion
    }
}
