// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.VGrains
{
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Attributes;
    using MARI.Framework.Core.Abstractions.Exceptions;
    using MARI.Framework.Node.Blackboard.Abstractions;
    using MARI.Framework.Node.Blackboard.Abstractions.Models;
    using MARI.Framework.Node.Blackboard.Abstractions.VGrains;
    using MARI.Framework.Node.Blackboard.Models;
    using MARI.Framework.Node.Blackboard.Models.Surrogates;

    using MARI.Toolbox.Abstractions.Services;
    using MARI.Toolbox.Collections;
    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using Orleans.Placement;
    using Orleans.Runtime;

    using System;
    using System.Security.Cryptography;
    using System.Threading.Tasks;

    /// <summary>
    /// Registry used to stored all the blackboard created
    /// </summary>
    /// <seealso cref="Orleans.Grain" />
    [KeepAlive]
    [MARISystemVGrain]
    internal sealed class BlackboardRegistryGrain : VGrainBase<BlackboardRegistryState, BlackboardRegistryStateSurrogate, BlackboardRegistryStateConverter, IBlackboardRegistryGrain>, IBlackboardRegistryGrain
    {
        #region Fields

        private readonly IBlackboardTemplateDefinitionProvider _blackboardTemplateDefinitionProvider;

        private readonly Dictionary<BlackboardId, GrainId> _grainIdCache;

        private readonly IGrainFactory _grainFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardRegistryGrain"/> class.
        /// </summary>
        public BlackboardRegistryGrain(ILogger<IBlackboardRegistryGrain> logger,
                                       [PersistentState(BlackboardConstants.BlackboardRegistryStorageKey, BlackboardConstants.BlackboardRegistryStorageConfigurationKey)] IPersistentState<BlackboardRegistryStateSurrogate> persistentState,
                                       IGrainOrleanFactory grainFactory,
                                       IBlackboardTemplateDefinitionProvider blackboardTemplateDefinitionProvider)

            : base(logger, persistentState)
        {
            this._blackboardTemplateDefinitionProvider = blackboardTemplateDefinitionProvider;
            this._grainIdCache = new Dictionary<BlackboardId, GrainId>();

            this._grainFactory = grainFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<Tuple<BlackboardId, GrainId>> GetOrCreateAsync(string boardName, string blackboardTemplateKey, GrainCancellationToken token, Guid? callContextId = null)
        {
            var boardId = this.State!.TryGet(boardName, blackboardTemplateKey);

            // GrainId is cached after BuildFromTemplateAsync is called
            if (boardId is not null && this._grainIdCache.TryGetValue(boardId.Value, out var grainIdCached))
            {
                return Tuple.Create(boardId.Value, grainIdCached);
            }

            var isNewBoard = boardId is null;

            boardId = this.State!.CreateNewBlackboardId(boardName, blackboardTemplateKey);
            var blackboardGrain = this._grainFactory.GetGrain<IBlackboardGrain>(boardId!.Value.Uid);
            var grainId = blackboardGrain.GetGrainId();

            try
            {
                await InitializaBlackboardGrainAsync(blackboardGrain, boardId.Value, blackboardTemplateKey, token, callContextId);
            }
            catch
            {
                // Build failed : roll back the registration of a new board to prevent returning an unbuilt board later
                if (isNewBoard)
                    this.State!.Unregister(boardId.Value.Uid);
                throw;
            }

            // Register only after a successful build
            this._grainIdCache[boardId.Value] = grainId;
            await PushStateAsync(default);

            return Tuple.Create(boardId.Value, grainId);
        }

        /// <inheritdoc />
        public Task<Tuple<BlackboardId, GrainId>?> TryGetAsync(string boardName, string blackboardTemplateKey)
        {
            Tuple<BlackboardId, GrainId>? result = null;
            var boardId = this.State!.TryGet(boardName, blackboardTemplateKey);

            // GrainId is cached after BuildFromTemplateAsync is called
            if (boardId is not null && this._grainIdCache.TryGetValue(boardId.Value, out var grainIdCached))
            {
                result = Tuple.Create(boardId.Value, grainIdCached);
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<Tuple<BlackboardId, GrainId>?> TryGetAsync(Guid uid)
        {
            Tuple<BlackboardId, GrainId>? result = null;
            var boardId = this.State!.TryGetAsync(uid);

            if (boardId is not null)
            {
                // The cache is in memory only, after registry reactivation derive the grain id from the persisted board uid
                if (!this._grainIdCache.TryGetValue(boardId.Value, out var grainId))
                    grainId = this._grainFactory.GetGrain<IBlackboardGrain>(boardId.Value.Uid).GetGrainId();

                result = Tuple.Create(boardId.Value, grainId);
            }

            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public async Task Unregister(Guid uid)
        {
            this.State!.Unregister(uid);
            await PushStateAsync(default);
        }

        #region Tools

        /// <summary>
        /// Initializas the blackboard grain.
        /// </summary>
        private async Task InitializaBlackboardGrainAsync(IBlackboardGrain blackboardGrain,
                                                          BlackboardId blackboardId,
                                                          string blackboardTemplateKey,
                                                          GrainCancellationToken token,
                                                          Guid? callContextId = null)
        {
            var tmpl = await this._blackboardTemplateDefinitionProvider.GetFirstValueAsync(f => string.Equals(f.UniqueTemplateName, blackboardTemplateKey), token.CancellationToken);

#pragma warning disable IDE0270 // Use coalesce expression
            if (tmpl == null)
                throw new MissingDefinitionException(typeof(BlackboardTemplateDefinition), blackboardTemplateKey);
#pragma warning restore IDE0270 // Use coalesce expression

            await blackboardGrain.BuildFromTemplateAsync(tmpl.Uid, blackboardId, token, callContextId);
        }

        #endregion

        #endregion
    }
}
