// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard
{
    using MARI.Framework.Core.Abstractions.Exceptions;
    using MARI.Framework.Node.Blackboard.Abstractions;
    using MARI.Framework.Node.Blackboard.Abstractions.Exceptions;
    using MARI.Framework.Node.Blackboard.Abstractions.Models;
    using MARI.Framework.Node.Blackboard.Abstractions.VGrains;
    using MARI.Framework.Node.Blackboard.VGrains;

    using MARI.Toolbox.Collections;
    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Extensions;
    using MARI.Toolbox.Helpers;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using Orleans.Runtime;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Get or provide a blackboad based on specific DeferredId or couple DisplayName + Template DisplayName
    /// </summary>
    /// <seealso cref="IBlackboardProvider" />
    internal sealed class BlackboardProvider : SafeDisposable, IBlackboardProvider
    {
        #region Fields

        private static readonly TimeSpan s_blackboardBuildAndRetreiveTimeout;
        private static readonly long s_proxyCacheIdleTimeoutMs = (long)TimeSpan.FromMinutes(30).TotalMilliseconds;
        private static readonly long s_proxyCachePurgeIntervalMs = (long)TimeSpan.FromMinutes(5).TotalMilliseconds;

        private readonly ILogger<IBlackboardProvider> _logger;

        private readonly MultiKeyDictionary<string, ProxyCacheEntry> _blackboardProxiesByName;
        private readonly Dictionary<Guid, ProxyCacheEntry> _blackboardProxies;
        private long _lastProxyCachePurgeTick;

        private readonly ReaderWriterLockSlim _blackboardProxyLockerRW;

        private readonly IBlackboardTemplateDefinitionProvider _blackboardTemplateDefinitionProvider;
        private readonly IBlackboardRegistryGrain _blackboardRegistry;
        private readonly IServiceProvider _serviceProvider;
        private readonly IGrainFactory _grainFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="BlackboardProvider"/> class.
        /// </summary>
        static BlackboardProvider()
        {
            s_blackboardBuildAndRetreiveTimeout = Debugger.IsAttached ? TimeSpan.FromMinutes(45) : TimeSpan.FromMinutes(1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardProvider"/> class.
        /// </summary>
        public BlackboardProvider(IBlackboardTemplateDefinitionProvider blackboardTemplateDefinitionProvider,
                                  IServiceProvider serviceProvider,
                                  IGrainFactory grainFactory,
                                  ILogger<IBlackboardProvider>? logger = null)
        {
            this._logger = logger ?? NullLogger<BlackboardProvider>.Instance;

            this._blackboardProxiesByName = new MultiKeyDictionary<string, ProxyCacheEntry>();
            this._blackboardProxies = new Dictionary<Guid, ProxyCacheEntry>();
            this._lastProxyCachePurgeTick = Environment.TickCount64;
            this._blackboardTemplateDefinitionProvider = blackboardTemplateDefinitionProvider;
            this._serviceProvider = serviceProvider;
            this._blackboardProxyLockerRW = new ReaderWriterLockSlim();
            this._grainFactory = grainFactory;
            this._blackboardRegistry = this._grainFactory.GetGrain<IBlackboardRegistryGrain>(IBlackboardRegistryGrain.GrainId);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask<IBlackboardRef> GetBlackboardAsync(string boardName,
                                                                  string boardTemplateConfigurationKey,
                                                                  CancellationToken token = default,
                                                                  Guid? callContextId = null)
        {
            ArgumentNullException.ThrowIfNull(boardName);
            ArgumentNullException.ThrowIfNull(boardTemplateConfigurationKey);

            var tmpl = await this._blackboardTemplateDefinitionProvider.GetValuesAsync(d => d.UniqueTemplateName == boardTemplateConfigurationKey, token);

#pragma warning disable IDE0270 // Use coalesce expression
            if (tmpl is null || !tmpl.Any())
                throw new MissingDefinitionException(typeof(BlackboardTemplateDefinition), boardTemplateConfigurationKey);
#pragma warning restore IDE0270 // Use coalesce expression

            if (tmpl.Count > 1)
            {
                throw new DefinitionException(typeof(BlackboardTemplateDefinition),
                                              boardTemplateConfigurationKey,
                                              "Multiple definition are not allowed",
                                              MARIErrorCodes.Build(MARIErrorCodes.Categories.Definition,
                                                                        MARIErrorCodes.PartType.Identifier,
                                                                        MARIErrorCodes.ErrorType.Multiple));
            }

            return await GetBlackboardByNameAsync(boardName, tmpl.First(), token, callContextId);
        }

        /// <inheritdoc />
        public ValueTask<IBlackboardRef> GetBlackboardAsync(Guid uid, CancellationToken token = default, Guid? callContextId = null)
        {
            // Method used to put the "ByUid" and "ByName" code side and slit public and private methods
            return GetBlackboardByUidAsync(uid, token, callContextId);
        }

        /// <inheritdoc />
        public async ValueTask<IBlackboardRef> GetBlackboardAsync(string boardName,
                                                                  Guid boardTemplateConfigurationUid,
                                                                  CancellationToken token = default,
                                                                  Guid? callContextId = null)
        {
            var tmpl = await this._blackboardTemplateDefinitionProvider.GetByKeyAsync(boardTemplateConfigurationUid, token);

#pragma warning disable IDE0270 // Use coalesce expression
            if (tmpl is null)
                throw new MissingDefinitionException(typeof(BlackboardTemplateDefinition), boardTemplateConfigurationUid.ToString());
#pragma warning restore IDE0270 // Use coalesce expression

            return await GetBlackboardByNameAsync(boardName, tmpl, token, callContextId);
        }

        #region Tools

        /// <summary>
        /// Gets or create the <see cref="IBlackboardRef"/>
        /// </summary>
        private ValueTask<IBlackboardRef> GetBlackboardByNameAsync(string boardName, BlackboardTemplateDefinition template, CancellationToken token, Guid? callContextId)
        {
            var keys = new string[2] { template.UniqueTemplateName, boardName };

            return GetBlackboardImplAsync(() =>
                                          {
                                              if (this._blackboardProxiesByName.TryGetValue(keys, out var entry))
                                                  return entry.Touch();
                                              return null;
                                          },
                                          async (t) => await FecthBlackboardByNames(boardName, template, t),
                                          () => new BlackboardMissingMARIException(null, boardName, template.UniqueTemplateName),
                                          token,
                                          callContextId);
        }

        /// <summary>
        /// Fecthes the blackboard by names from registry
        /// </summary>
        private async Task<Tuple<BlackboardId, GrainId>> FecthBlackboardByNames(string boardName, BlackboardTemplateDefinition template, GrainCancellationToken token)
        {
            var result = await this._blackboardRegistry.TryGetAsync(boardName, template.UniqueTemplateName);

            if (result is null)
                result = await this._blackboardRegistry.GetOrCreateAsync(boardName, template.UniqueTemplateName, token);

            return result;
        }

        /// <inheritdoc />
        private ValueTask<IBlackboardRef> GetBlackboardByUidAsync(Guid uid, CancellationToken token, Guid? callContextId)
        {
            return GetBlackboardImplAsync(() =>
                                          {
                                              if (this._blackboardProxies.TryGetValue(uid, out var entry))
                                                  return entry.Touch();
                                              return null;
                                          },
                                          async (_) => await this._blackboardRegistry.TryGetAsync(uid),
                                          () => new BlackboardMissingMARIException(uid, null, null),
                                          token,
                                          callContextId);
        }

        /// <inheritdoc />
        private async ValueTask<IBlackboardRef> GetBlackboardImplAsync(Func<BlackboardHandlerProxy?> fetchFromCache,
                                                                       Func<GrainCancellationToken, Task<Tuple<BlackboardId, GrainId>?>> getBlackboardFromRegistry,
                                                                       Func<BlackboardMissingMARIException> formatMissingException,
                                                                       CancellationToken token,
                                                                       Guid? callContextId)
        {
            try
            {
                using (var timeout = CancellationHelper.DisposableTimeout(s_blackboardBuildAndRetreiveTimeout))
                using (var grp = CancellationTokenSource.CreateLinkedTokenSource(timeout.Content, token))
                using (var grainCancelSource = grp.ToGrainCancellationTokenSource())
                {
                    this._blackboardProxyLockerRW.EnterReadLock();
                    try
                    {
                        var proxy = fetchFromCache();
                        if (proxy is not null)
                            return proxy;
                    }
                    finally
                    {
                        this._blackboardProxyLockerRW.ExitReadLock();
                    }

                    IBlackboardRef? result = null;
                    bool isNew = false;

                    var tpl = await getBlackboardFromRegistry(grainCancelSource.Token);

                    if (tpl is null)
                        throw formatMissingException();

                    var boardId = tpl.Item2;
                    var blackboardId = tpl.Item1;

                    this._blackboardProxyLockerRW.EnterWriteLock();
                    try
                    {
                        var proxy = fetchFromCache();
                        if (proxy is not null)
                            return proxy;

                        var newProxy = ActivatorUtilities.CreateInstance<BlackboardHandlerProxy>(this._serviceProvider, boardId, blackboardId);

                        var keys = new string[2] { blackboardId.BoardTemplateKey, blackboardId.BoardName };
                        var uid = blackboardId.Uid;

                        // Evict proxies not used for a while to prevent unbounded growth and stale proxies
                        PurgeIdleProxiesNoLock();

                        // Attention : In Theory could override a target
                        var entry = new ProxyCacheEntry(newProxy, uid, keys);
                        this._blackboardProxies[uid] = entry;
                        this._blackboardProxiesByName[keys] = entry;

                        isNew = true;
                        result = newProxy;
                    }
                    finally
                    {
                        this._blackboardProxyLockerRW.ExitWriteLock();
                    }

                    if (isNew)
                        await CheckBlackboardInitializedAsync(result, token, callContextId);

                    return result;
                }
            }
            catch (Exception ex)
            {
                this._logger.OptiLog(LogLevel.Error, "Provider {exception}", ex);
                throw;
            }
        }

        /// <summary>
        /// Removes from the cache all proxies not accessed since <see cref="s_proxyCacheIdleTimeoutMs"/>.
        /// </summary>
        /// <remarks>
        ///     Must be called under write lock.
        ///     An evicted proxy is simply re-created from the registry on next request.
        /// </remarks>
        private void PurgeIdleProxiesNoLock()
        {
            var now = Environment.TickCount64;
            if (now - this._lastProxyCachePurgeTick < s_proxyCachePurgeIntervalMs)
                return;

            this._lastProxyCachePurgeTick = now;

            var idleEntries = this._blackboardProxies.Values
                                                     .Where(e => now - e.LastAccessTick > s_proxyCacheIdleTimeoutMs)
                                                     .ToArray();

            foreach (var idle in idleEntries)
            {
                if (this._blackboardProxies.TryGetValue(idle.Uid, out var byUid) && ReferenceEquals(byUid, idle))
                    this._blackboardProxies.Remove(idle.Uid);

                if (this._blackboardProxiesByName.TryGetValue(idle.NameKeys, out var byName) && ReferenceEquals(byName, idle))
                    this._blackboardProxiesByName.Remove(idle.NameKeys);
            }
        }

        /// <summary>
        /// Checks the blackboard initialized asynchronous.
        /// </summary>
        private async Task CheckBlackboardInitializedAsync(IBlackboardRef result, CancellationToken token, Guid? callContextId)
        {
            var status = await result.GetStatusAsync(token);
            if (status == BlackboardLifeStatusEnum.None)
                await result.InitializeAsync(token, callContextId);
        }

        /// <inheritdoc />
        protected override void DisposeEnd()
        {
            this._blackboardProxyLockerRW.Dispose();
            this._blackboardProxiesByName.Dispose();
            base.DisposeEnd();
        }

        #endregion

        #region Nested

        /// <summary>
        /// Proxy cache entry tracking last access time
        /// </summary>
        private sealed class ProxyCacheEntry
        {
            private long _lastAccessTick;

            public ProxyCacheEntry(BlackboardHandlerProxy proxy, Guid uid, IReadOnlyCollection<string> nameKeys)
            {
                this.Proxy = proxy;
                this.Uid = uid;
                this.NameKeys = nameKeys;
                this._lastAccessTick = Environment.TickCount64;
            }

            public BlackboardHandlerProxy Proxy { get; }

            public Guid Uid { get; }

            public IReadOnlyCollection<string> NameKeys { get; }

            public long LastAccessTick
            {
                get { return Interlocked.Read(ref this._lastAccessTick); }
            }

            /// <summary>
            /// Refresh last access time and return the proxy
            /// </summary>
            public BlackboardHandlerProxy Touch()
            {
                Interlocked.Exchange(ref this._lastAccessTick, Environment.TickCount64);
                return this.Proxy;
            }
        }

        #endregion

        #endregion
    }
}
