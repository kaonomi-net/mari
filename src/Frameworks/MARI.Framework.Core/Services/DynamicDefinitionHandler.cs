// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Services
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Models;
    using MARI.Framework.Core.Abstractions.Services;

    using MARI.Toolbox.Abstractions.Conditions;
    using MARI.Toolbox.Models;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <inheritdoc cref="IDynamicDefinitionHandler" />
    internal sealed class DynamicDefinitionHandler : IDynamicDefinitionHandler
    {
        #region Fields

        private readonly IVGrainMARISystemProvider _mariSystemProvider;
        private readonly ILogger<IDynamicDefinitionHandler>? _logger;

        private IDynamicDefinitionHandlerVGrain? _cachedHandler;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDefinitionHandler"/> class.
        /// </summary>
        public DynamicDefinitionHandler(IVGrainMARISystemProvider mariSystemProvider,
                                        ILogger<IDynamicDefinitionHandler> logger)
        {
            this._mariSystemProvider = mariSystemProvider;
            this._logger = logger ?? NullLogger<IDynamicDefinitionHandler>.Instance;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<bool> ChangeStatus(Guid definitionUid, bool isEnabled, IIdentityCard identity, CancellationToken token)
        {
            var grain = await GetHandlerVGrainAsync();
            using (var grainCancelToken = token.ToGrainCancellationTokenSource())
            {
                return await grain.ChangeStatus(definitionUid, isEnabled, identity, grainCancelToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<TDefinition?> GetDefinitionAsync<TDefinition>(Guid uid, CancellationToken token) 
            where TDefinition : class, IDefinition
        {
            return (await GetDefinitionAsync<TDefinition>(token, uid)).Info.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<EtagContainer<IReadOnlyCollection<TDefinition>>> GetDefinitionAsync<TDefinition>(CancellationToken token, params Guid[] uids) 
            where TDefinition : class, IDefinition
        {
            var grain = await GetHandlerVGrainAsync();
            using (var grainCancelToken = token.ToGrainCancellationTokenSource())
            {
                return await grain.GetDefinitionAsync<TDefinition>(grainCancelToken.Token, uids);
            }
        }

        /// <inheritdoc />
        public async Task<EtagContainer<IReadOnlyCollection<TDefinition>>> GetDefinitionAsync<TDefinition>(Expression<Func<TDefinition, bool>> filter, CancellationToken token) 
            where TDefinition : class, IDefinition
        {
            var grain = await GetHandlerVGrainAsync();
            using (var grainCancelToken = token.ToGrainCancellationTokenSource())
            {
                return await grain.GetDefinitionAsync<TDefinition>(filter.Serialize(), grainCancelToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<EtagContainer<IReadOnlyCollection<DynamicDefinitionMetaData>>> GetDynamicDefinitionMetaDatasAsync(ConcretType? typeFilter = null, string? displayNameRegex = null, bool onlyEnabled = false, CancellationToken token = default)
        {
            var grain = await GetHandlerVGrainAsync();
            using (var grainCancelToken = token.ToGrainCancellationTokenSource())
            {
                return await grain.GetDynamicDefinitionMetaDatasAsync(typeFilter, displayNameRegex, onlyEnabled, grainCancelToken.Token);
            }
        }

        /// <inheritdoc />
        public async Task<string> GetHandlerEtagAsync()
        {
            var grain = await GetHandlerVGrainAsync();
            return await grain.GetHandlerEtagAsync();
        }

        /// <inheritdoc />
        public async Task<Guid> PushDefinitionAsync<TDefinition>(ConditionExpressionDefinition existFilter, TDefinition definition, IIdentityCard identity, CancellationToken token, bool preventNotification = false)
            where TDefinition : class, IDefinition
        {
            var grain = await GetHandlerVGrainAsync();
            using (var grainCancelToken = token.ToGrainCancellationTokenSource())
            {
                return await grain.PushDefinitionAsync<TDefinition>(existFilter, definition, identity, grainCancelToken.Token, preventNotification);
            }
        }

        /// <inheritdoc />
        public Task<Guid> PushDefinitionAsync<TDefinition>(Expression<Func<TDefinition, bool>> filter, TDefinition definition, IIdentityCard identity, CancellationToken token, bool preventNotification = false)
            where TDefinition : class, IDefinition
        {
            return PushDefinitionAsync<TDefinition>(filter.Serialize(), definition, identity, token, preventNotification);
        }

        /// <inheritdoc />
        public async Task<bool> PushDefinitionAsync<TDefinition>(TDefinition definition, bool @override, IIdentityCard identity, CancellationToken token, bool preventNotification = false)
            where TDefinition : class, IDefinition
        {
            var grain = await GetHandlerVGrainAsync();
            using (var grainCancelToken = token.ToGrainCancellationTokenSource())
            {
                return await grain.PushDefinitionAsync(definition, @override, identity, grainCancelToken.Token, preventNotification);
            }
        }

        /// <inheritdoc />
        public Task<bool> RemoveDefinitionAsync(Guid definitionId, IIdentityCard identity, CancellationToken token)
        {
            return RemoveDefinitionAsync(identity, token, definitionId);
        }

        /// <inheritdoc />
        public async Task<bool> RemoveDefinitionAsync(IIdentityCard identity, CancellationToken token, params Guid[] definitionIds)
        {
            var grain = await GetHandlerVGrainAsync();
            using (var grainCancelToken = token.ToGrainCancellationTokenSource())
            {
                return await grain.RemoveDefinitionAsync(identity, grainCancelToken.Token, definitionIds);
            }
        }

        #region Tools

        /// <summary>
        /// Gets the handler VGrain.
        /// </summary>
        private async ValueTask<IDynamicDefinitionHandlerVGrain> GetHandlerVGrainAsync()
        {
            var handlerCached = this._cachedHandler;

            if (handlerCached is null)
            {
                handlerCached = await this._mariSystemProvider.GetVGrainAsync<IDynamicDefinitionHandlerVGrain>(null, this._logger);
                this._cachedHandler = handlerCached;
            }

            return handlerCached;
        }

        #endregion

        #endregion
    }
}
