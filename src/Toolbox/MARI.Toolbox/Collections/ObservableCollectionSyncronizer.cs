// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Collections
{
    using MARI.Toolbox.Abstractions.Proxies;
    using MARI.Toolbox.Abstractions.Supports;
    using MARI.Toolbox.Disposables;

    using System.Collections.ObjectModel;

    /// <summary>
    /// 
    /// </summary>
    public sealed class ObservableCollectionSyncronizer<TItem, TKey> : SafeAsyncDisposable
        where TKey : IEquatable<TKey>
    {
        #region Fields

        private readonly ObservableCollection<TItem> _observableCollection;
        private readonly Dictionary<TKey, TItem> _indexedItems;
        private readonly IDispatcherProxy _dispatcherProxy;
        private readonly Func<TItem, TKey> _keyAccess;
        private readonly bool _disposeContent;
        private readonly SemaphoreSlim _locker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionSyncronizer{TItem, TKey}"/> class.
        /// </summary>
        public ObservableCollectionSyncronizer(IDispatcherProxy dispatcherProxy,
                                               Func<TItem, TKey> keyAccess,
                                               bool disposeContent = true)
            : this(dispatcherProxy, null, null, keyAccess, disposeContent)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionSyncronizer{TItem, TKey}"/> class.
        /// </summary>
        public ObservableCollectionSyncronizer(IDispatcherProxy dispatcherProxy,
                                               ObservableCollection<TItem> source,
                                               Func<TItem, TKey> keyAccess,
                                               bool disposeContent = true)
            : this(dispatcherProxy, null, source, keyAccess, disposeContent)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionSyncronizer{TItem, TKey}"/> class.
        /// </summary>
        public ObservableCollectionSyncronizer(IDispatcherProxy dispatcherProxy,
                                               IEnumerable<TItem> source,
                                               Func<TItem, TKey> keyAccess,
                                               bool disposeContent = true)
            : this(dispatcherProxy, source, null, keyAccess, disposeContent)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionSyncronizer{TItem, TKey}"/> class.
        /// </summary>
        private ObservableCollectionSyncronizer(IDispatcherProxy dispatcherProxy,
                                                IEnumerable<TItem>? source,
                                                ObservableCollection<TItem>? sourceObservable,
                                                Func<TItem, TKey> keyAccess,
                                                bool disposeContent)
        {
            this._dispatcherProxy = dispatcherProxy;
            this._keyAccess = keyAccess;

            this._disposeContent = disposeContent;
            this._locker = new SemaphoreSlim(1);

            if (sourceObservable is not null)
                this._observableCollection = sourceObservable;
            else if (source is not null)
                this._observableCollection = new ObservableCollection<TItem>(source);
            else
                this._observableCollection = new ObservableCollection<TItem>();

            this._indexedItems = this._observableCollection!.ToDictionary(k => keyAccess(k));
            this.ReadOnly = new ReadOnlyObservableCollection<TItem>(this._observableCollection);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the read only.
        /// </summary>
        public IReadOnlyCollection<TItem> ReadOnly { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Manually inject an item
        /// </summary>
        public ValueTask<bool> ManualInject(TItem item, CancellationToken token = default)
        {
            return ManualInject(item, i => i, this._keyAccess);
        }

        /// <summary>
        /// Manually inject an item
        /// </summary>
        public async ValueTask<bool> ManualInject<TOther>(TOther item, Func<TOther, TItem> converter, Func<TOther, TKey> otherKeyAccess, CancellationToken token = default)
        {
            var key = otherKeyAccess(item);
            TItem? newItem = default;

            await this._locker.WaitAsync(token);
            try
            {
                if (this._indexedItems.TryGetValue(key, out var target))
                {
                    if (target is ISupportUpdateAsync<TOther> updateTargetAsync)
                        await updateTargetAsync.UpdateAsync(item);
                    else if (target is ISupportUpdate<TOther> updateTarget)
                        updateTarget.Update(item);

                    return true;
                }

                newItem = converter(item);

                if (newItem is ISupportUpdateAsync<TOther> newItemUpdateAsync)
                    await newItemUpdateAsync.UpdateAsync(item);
                else if (newItem is ISupportUpdate<TOther> newItemUpdate)
                    newItemUpdate.Update(item);

                token.ThrowIfCancellationRequested();

                this._indexedItems.Add(key, newItem);
            }
            finally
            {
                this._locker.Release();
            }

            this._dispatcherProxy.Send(() => this._observableCollection.Add(newItem));

            return true;
        }

        /// <summary>
        /// Add or remove the expose collection to the one pass in arguments
        /// </summary>
        public ValueTask<bool> Synchronize(IReadOnlyCollection<TItem> items, CancellationToken token = default)
        {
            return Synchronize(items, i => i, this._keyAccess);
        }

        /// <summary>
        /// Add or remove the expose collection to the one pass in arguments
        /// </summary>
        public async ValueTask<bool> Synchronize<TOther>(IReadOnlyCollection<TOther> items, Func<TOther, TItem> converter, Func<TOther, TKey> otherKeyAccess, CancellationToken token = default)
        {
            var indexItems = items.GroupBy(i => otherKeyAccess(i))
                                  .ToDictionary(k => k.Key, v => v.Single());

            var addItems = new List<TItem>(items.Count);
            var removeItems = new List<TItem>(items.Count);

            var existingItems = new HashSet<TKey>(this._indexedItems.Keys);

            await this._locker.WaitAsync(token);
            try
            {
                foreach (var kv in indexItems)
                {
                    if (this._indexedItems.TryGetValue(kv.Key, out var target))
                    {
                        existingItems.Remove(kv.Key);

                        if (target is ISupportUpdateAsync<TOther> updateTargetAsync)
                            await updateTargetAsync.UpdateAsync(kv.Value);
                        else if (target is ISupportUpdate<TOther> updateTarget)
                            updateTarget.Update(kv.Value);

                        continue;
                    }

                    var newItem = converter(kv.Value);

                    if (newItem is ISupportUpdateAsync<TOther> newItemUpdateAsync)
                        await newItemUpdateAsync.UpdateAsync(kv.Value);
                    else if (newItem is ISupportUpdate<TOther> newItemUpdate)
                        newItemUpdate.Update(kv.Value);

                    addItems.Add(newItem);
                }

                token.ThrowIfCancellationRequested();

                foreach (var removeKey in existingItems)
                {
                    if (this._indexedItems.Remove(removeKey, out var oldItem))
                        removeItems.Add(oldItem);
                }

                foreach (var newItem in addItems)
                    this._indexedItems.Add(this._keyAccess(newItem), newItem);
            }
            finally
            {
                this._locker.Release();
            }

            this._dispatcherProxy.Send(() =>
            {
                foreach (var it in removeItems)
                    this._observableCollection.Remove(it);

                foreach (var it in addItems)
                    this._observableCollection.Add(it);
            });

            return addItems.Any() || existingItems.Any();
        }

        #region Tools

        /// <inheritdoc />
        protected override async ValueTask DisposeBeginAsync()
        {
            TItem[] items;

            await this._locker.WaitAsync();
            try
            {
                items = this._observableCollection.ToArray();
                this._observableCollection.Clear();
                this._indexedItems.Clear();
            }
            finally
            {
                this._locker.Release();
            }

            if (this._disposeContent)
            {
                List<Exception>? exceptions = null;

                foreach (var item in items)
                {
                    try
                    {
                        if (item is IDisposable disposable)
                            disposable.Dispose();
                        else if (item is IAsyncDisposable asyncDisposable)
                            await asyncDisposable.DisposeAsync();
                    }
                    catch (Exception ex)
                    {
                        exceptions = exceptions.AddOnNull(ex);
                    }
                }

                if (exceptions is not null && exceptions.Count > 0)
                    throw new AggregateException(exceptions);
            }

            await base.DisposeBeginAsync();
        }

        /// <inheritdoc />
        protected override ValueTask DisposeEndAsync()
        {
            this._locker.Dispose();
            return base.DisposeBeginAsync();
        }

        #endregion

        #endregion
    }
}
