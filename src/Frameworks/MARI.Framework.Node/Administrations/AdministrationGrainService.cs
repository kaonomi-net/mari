// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Administrations
{
    using MARI.Framework.Core.Abstractions.Services;
    using MARI.Framework.Node.Abstractions.Administrations;
    using MARI.Framework.Node.Abstractions.Enums;
    using MARI.Framework.Node.Abstractions.Models.Administrations;
    using MARI.Framework.Node.Services;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Threading.Tasks;

    internal sealed class AdministrationGrainService : MARIVGrainService, IAdministrationGrainService
    {
        #region Fields

        private readonly IVGrainMARISystemProvider _mariSystemProvider;
        private readonly ILogger<IAdministrationGrainService> _localLogger;

        private readonly GrainRouteSiloRootService _grainRouteSiloRootService;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AdministrationGrainService"/> class.
        /// </summary>
        public AdministrationGrainService(GrainId grainId,
                                          Silo silo,
                                          ILoggerFactory loggerFactory,
                                          IVGrainMARISystemProvider mariSystemProvider,
                                          GrainRouteSiloRootService grainRouteSiloRootService)
            : base(grainId, silo, loggerFactory)
        {
            this._mariSystemProvider = mariSystemProvider;
            this._localLogger = loggerFactory.CreateLogger<IAdministrationGrainService>();
            this._grainRouteSiloRootService = grainRouteSiloRootService;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override async Task RefreshInfoAsync()
        {
            using (var cancellation = new GrainCancellationTokenSource())
            {
                var clusterRoute = await this._mariSystemProvider.GetVGrainAsync<IClusterRouteRegistryVGrain>(null, this._localLogger);
                await clusterRoute.SubscribeRouteChangeAsync(this.GetDedicatedGrainId<IAdminEventReceiver>(), cancellation.Token);
            }
        }

        /// <inheritdoc />
        public async Task ReceiveAsync<TEvent>(TEvent adminEvent, GrainCancellationToken cancellationToken) where TEvent : AdminEventArg
        {
            if (adminEvent is null)
                return;

            if (adminEvent.Type == AdminEventTypeEnum.RouteChange)
            {
                await this._grainRouteSiloRootService.UpdateGlobalRedirectionAsync(adminEvent.Etag, cancellationToken.CancellationToken);
                return;
            }

            throw new NotImplementedException();
        }

        #endregion
    }
}
