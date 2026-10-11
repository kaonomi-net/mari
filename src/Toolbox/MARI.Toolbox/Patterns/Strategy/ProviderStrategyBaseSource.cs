// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Strategy
{
    using MARI.Toolbox.Abstractions.Patterns.Strategy;
    using MARI.Toolbox.Supports;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Base implementation of <see cref="IProviderStrategySource{T, TKey}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <seealso cref="IProviderStrategySource{T, TKey}" />
    public abstract class ProviderStrategyBaseSource<TValue, TKey> : SupportBaseInitialization<IServiceProvider>, IProviderStrategySource<TValue, TKey>
        where TValue : class
        where TKey : notnull
    {
        #region Fields

        private readonly IServiceProvider _serviceProvider;

        private readonly Dictionary<TKey, TValue> _cachedData;
        private readonly ReaderWriterLockSlim _dataCacheLock;
        private readonly bool _supportFallback;
        private readonly HashSet<TKey> _keys;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderStrategyBaseSource{TValue, TKey}"/> class.
        /// </summary>
        protected ProviderStrategyBaseSource(IServiceProvider serviceProvider,
                                             IEnumerable<(TKey key, TValue value)>? initValues = null,
                                             bool supportFallback = false)
        {
            this._serviceProvider = serviceProvider;

            var readOnlyInitValues = initValues?.Select(kv => new KeyValuePair<TKey, TValue>(kv.key, kv.value))
                                               ?? EnumerableHelper<KeyValuePair<TKey, TValue>>.ReadOnly;

            this._cachedData = new Dictionary<TKey, TValue>(readOnlyInitValues);
            this._dataCacheLock = new ReaderWriterLockSlim();

            this._supportFallback = supportFallback;
            this._keys = new HashSet<TKey>();
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public IReadOnlyCollection<TKey> Keys
        {
            get
            {
                this._dataCacheLock.EnterReadLock();
                try
                {
                    return this._keys;
                }
                finally
                {
                    this._dataCacheLock?.ExitReadLock();
                }
            }
        }

        #endregion

        #region Events

        /// <inheritdoc />
        public event EventHandler<IReadOnlyCollection<TKey>>? DataChanged;

        #endregion

        #region Methods

        /// <inheritdoc />
        public virtual async ValueTask<IReadOnlyCollection<TValue>> GetAllValuesAsync(CancellationToken token)
        {
            await EnsureProviderIsInitialized();

            return await ExecWithRetry<IReadOnlyCollection<TValue>>(() =>
            {
                this._dataCacheLock.EnterReadLock();
                try
                {
                    var result = this._cachedData.Select(kv => kv.Value)
                                                 .ToArray();

                    return result;
                }
                finally
                {
                    this._dataCacheLock.ExitReadLock();
                }
            },
            r => r is null || r.Count == 0,
            token) ?? EnumerableHelper<TValue>.ReadOnlyArray;
        }

        /// <inheritdoc />
        public virtual async ValueTask<(bool Success, TValue? Result)> TryGetDataAsync(TKey key, CancellationToken token)
        {
            await EnsureProviderIsInitialized();

            return await ExecWithRetry<(bool Success, TValue? Result)>(() =>
            {
                this._dataCacheLock.EnterReadLock();
                try
                {
                    if (this._cachedData.TryGetValue(key, out var cachedValue))
                        return (Success: true, Result: cachedValue);
                }
                finally
                {
                    this._dataCacheLock.ExitReadLock();
                }

                return (false, default(TValue));
            },
            r => r.Success == false,
            token);
        }

        /// <inheritdoc />
        public virtual async ValueTask<IReadOnlyCollection<TValue>> GetValuesAsync(Expression<Func<TValue, bool>> filterExpression, Func<TValue, bool> filter, CancellationToken token)
        {
            await EnsureProviderIsInitialized();

            return await ExecWithRetry<IReadOnlyCollection<TValue>>(() =>
            {
                this._dataCacheLock.EnterReadLock();
                try
                {
                    var result = this._cachedData.Where(kv => filter(kv.Value))
                                                 .Select(kv => kv.Value)
                                                 .ToArray();

                    return result;
                }
                finally
                {
                    this._dataCacheLock.ExitReadLock();
                }
            },
            r => r is null || r.Count == 0,
            token) ?? EnumerableHelper<TValue>.ReadOnlyArray;
        }

        /// <inheritdoc />
        public virtual async ValueTask<TValue?> GetFirstValueAsync(Expression<Func<TValue, bool>> filterExpression, Func<TValue, bool> filter, CancellationToken token)
        {
            await EnsureProviderIsInitialized();

            return await ExecWithRetry<TValue>(() =>
            {
                this._dataCacheLock.EnterReadLock();
                try
                {
                    var result = this._cachedData.Where(kv => filter(kv.Value))
                                                 .Select(kv => kv.Value)
                                                 .FirstOrDefault();

                    return result;
                }
                finally
                {
                    this._dataCacheLock.ExitReadLock();
                }
            },
            r => r is null,
            token);
        }

        /// <inheritdoc />
        public virtual async ValueTask<IReadOnlyCollection<TKey>> GetKeysAsync(Expression<Func<TValue, bool>> filterExpression, Func<TValue, bool> filter, CancellationToken token = default)
        {
            await EnsureProviderIsInitialized();

            return await ExecWithRetry<IReadOnlyCollection<TKey>>(() =>
            {
                this._dataCacheLock.EnterReadLock();
                try
                {
                    var result = this._cachedData.Where(kv => filter(kv.Value))
                                                 .Select(kv => kv.Key)
                                                 .ToArray();

                    return result;
                }
                finally
                {
                    this._dataCacheLock.ExitReadLock();
                }
            },
            r => r is null || r.Count == 0,
            token) ?? EnumerableHelper<TKey>.ReadOnlyArray;
        }

        /// <inheritdoc />
        public virtual async ValueTask<IReadOnlyCollection<TValue>> GetValuesAsync(IEnumerable<TKey> keys, CancellationToken token)
        {
            await EnsureProviderIsInitialized();

            return await ExecWithRetry<IReadOnlyCollection<TValue>>(() =>
            {
                this._dataCacheLock.EnterReadLock();
                try
                {
                    var results = keys.Distinct()
                                      .Where(k => this._cachedData.ContainsKey(k))
                                      .Select(k => this._cachedData[k])
                                      .ToReadOnly();

                    return results;
                }
                finally
                {
                    this._dataCacheLock.ExitReadLock();
                }
            },
            r => r is null || r.Count == 0,
            token) ?? EnumerableHelper<TValue>.ReadOnlyArray;
        }

        /// <inheritdoc />
        public async ValueTask ForceUpdateAsync(CancellationToken token)
        {
            await EnsureProviderIsInitialized();
            await ForceUpdateAfterInitAsync(token);
        }

        /// <inheritdoc />
        protected sealed override async ValueTask OnInitializingAsync(IServiceProvider? serviceProvider, CancellationToken token)
        {
            await OnProviderInitializedAsync(serviceProvider, token);
            await ForceUpdateAfterInitAsync(token);
        }

        /// <summary>
        /// Forces the update (all initialization have been done doing it could create a dead lock).
        /// </summary>
        protected virtual ValueTask ForceUpdateAfterInitAsync(CancellationToken token)
        {
            return ValueTask.CompletedTask;
        }

        #region Tools

        /// <summary>
        /// Ensures the provider is initialized.
        /// </summary>
        protected ValueTask EnsureProviderIsInitialized()
        {
            if (this.IsInitialized)
                return ValueTask.CompletedTask;

            return InitializationAsync(this._serviceProvider, default);
        }

        /// <inheritdoc />
        protected virtual ValueTask OnProviderInitializedAsync(IServiceProvider? serviceProvider, CancellationToken token)
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Thread safes add a key, value
        /// </summary>
        protected void SafeAddOrReplace(TKey key, TValue value)
        {
            SafeAddOrReplace((key, value).AsEnumerable());
        }

        /// <summary>
        /// Thread safes add a key, values
        /// </summary>
        protected void SafeAddOrReplace(IEnumerable<(TKey key, TValue value)> values)
        {
            ArgumentNullException.ThrowIfNull(values);

            this._dataCacheLock.EnterWriteLock();
            try
            {
                foreach (var kv in values)
                {
                    if (this._cachedData.TryAdd(kv.key, kv.value))
                    {
                        this._keys.Add(kv.key);
                    }
                    else
                    {
                        this._cachedData[kv.key] = kv.value;
                    }
                }
            }
            finally
            {
                this._dataCacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Safes the clear all recorded data.
        /// </summary>
        protected void SafeClear()
        {
            ArgumentNullException.ThrowIfNull(this._keys);

            this._dataCacheLock.EnterWriteLock();
            try
            {
                this._cachedData.Clear();
                this._keys.Clear();
            }
            finally
            {
                this._dataCacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Thread safes add a key, value
        /// </summary>
        protected void SafeRemoves(IEnumerable<TKey> keys)
        {
            ArgumentNullException.ThrowIfNull(keys);

            this._dataCacheLock.EnterWriteLock();
            try
            {
                foreach (var key in keys)
                {
                    if (this._cachedData.Remove(key))
                        this._keys.Remove(key);
                }
            }
            finally
            {
                this._dataCacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Raiseds <see cref="DataChanged"/>.
        /// </summary>
        protected void RaisedDataChanged(IReadOnlyCollection<TKey> definitionThatChanged)
        {
            DataChanged?.Invoke(this, definitionThatChanged);
        }

        /// <summary>
        /// Execute a func with a fallback when failed
        /// </summary>
        protected async ValueTask<TResult?> ExecWithRetry<TResult>(Func<TResult?> execFunc, Func<TResult?, bool> failCheck, CancellationToken token)
        {
            return await ExecWithRetryAsync(() =>
            {
                var result = execFunc();
                return ValueTask.FromResult<TResult?>(result);
            },
            failCheck,
            token);
        }

        /// <summary>
        /// Execute a func with a fallback when failed
        /// </summary>
        protected async ValueTask<TResult?> ExecWithRetryAsync<TResult>(Func<ValueTask<TResult?>> execFunc, Func<TResult?, bool> failCheck, CancellationToken token)
        {
            Exception? exception = null;
            TResult? result = default;

            try
            {
                result = await execFunc();
            }
            catch (Exception ex)
            {
                if (!this._supportFallback)
                    throw;

                exception = ex;
            }

            if (this._supportFallback && (exception is not null || failCheck(result)))
            {
                await FallbackOdRetryFailedAsync(token);

                result = await execFunc();
            }

            return result;
        }

        /// <summary>
        /// Call when a result failed to try to fix
        /// </summary>
        protected virtual Task FallbackOdRetryFailedAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        #endregion

        #endregion
    }
}
