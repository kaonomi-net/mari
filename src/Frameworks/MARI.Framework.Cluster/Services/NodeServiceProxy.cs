// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Cluster.Services
{
    using MARI.Framework.Core.Abstractions;

    using MARI.Toolbox.Supports;

    using Microsoft.Extensions.DependencyInjection;

    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Service proxy used to initialize and finalize
    /// </summary>
    internal sealed class NodeServiceProxy : SupportBaseInitialization<IServiceProvider>, IInitService, IFinalizeService
    {
        #region Fields

        private readonly IServiceProvider _serviceProvider;
        private readonly Type _serviceKey;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeServiceProxy"/> class.
        /// </summary>
        public NodeServiceProxy(IServiceProvider serviceProvider, Type serviceKey)
        {
            this._serviceProvider = serviceProvider;
            this._serviceKey = serviceKey;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool IsFinalizing
        {
            get { return false; }
        }

        /// <inheritdoc />
        public bool IsFinalized
        {
            get { return false; }
        }


        /// <inheritdoc />
        public bool ExpectOrleanStarted
        {
            get { return false; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask FinalizeAsync(CancellationToken token = default)
        {
            var services = this._serviceProvider.GetServices(this._serviceKey)
                                                .OfType<IFinalizeService>()
                                                .ToArray();

            var tasks = services.Select(s => s.FinalizeAsync(token))
                                .ToArray();

            await tasks.SafeWhenAllAsync(token);
        }

        /// <inheritdoc />
        protected override async ValueTask OnInitializingAsync(IServiceProvider? serviceProvider, CancellationToken token)
        {
            var services = this._serviceProvider.GetServices(this._serviceKey)
                                                .OfType<IInitService>()
                                                .ToArray();

            var tasks = services.Select(s => s.InitializationAsync(serviceProvider, token))
                                .ToArray();

            await tasks.SafeWhenAllAsync(token);
        }

        #endregion
    }
}
