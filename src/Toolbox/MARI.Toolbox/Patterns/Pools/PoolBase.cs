// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Pools
{
    using MARI.Toolbox.Abstractions.Patterns.Pools;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    /// <summary>
    /// Base class of pool pattern implementation
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <seealso cref="IPool{TItem}" />
    public abstract class PoolBase<TItem> : IPool<TItem>
        where TItem : class, IPoolItem, new()
    {
        #region Fields

        private readonly Queue<TItem> _items;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="PoolBase{TItem}"/> class.
        /// </summary>
        protected PoolBase(int maxRemainItems, int? initItems = null)
        {
            this._items = new Queue<TItem>(Math.Max(42, (initItems is not null && initItems > 0 ? initItems.Value : 42)));
            this.MaxRemainItems = maxRemainItems;

            if (initItems is not null && initItems > 0)
            {
                for (int i = 0; i < initItems.Value; i++)
                {
                    var item = new TItem();
                    this._items.Enqueue(item);
                }
            }
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public virtual int MaxRemainItems { get; }

        #endregion

        /// <inheritdoc />
        public TItem GetItem()
        {
            return GetItems(1).Single();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<TItem> GetItems(ushort count)
        {
            TItem[] newItems;

            using (CreateSafeContext())
            {
                newItems = Enumerable.Range(0, count)
                                     .Select(_ =>
                                     {
                                         TItem? item = null;

                                         if (this._items.TryDequeue(out var dequeueItem))
                                             item = dequeueItem;

                                         return item ?? new TItem();
                                     })
                                     .ToArray();
            }

            if (count > 10_000)
            {
                Parallel.ForEach(newItems, item => item.Prepare(this));
            }
            else
            {
                foreach (var item in newItems)
                {
                    Debug.Assert(item.InUse == false || item.InUse == null);
                    item.Prepare(this);
                    Debug.Assert(item.InUse == true);
                }
            }

            return newItems;
        }

        /// <inheritdoc />
        public void Recycle<T>(T item)
            where T : IPoolItem
        {
            // Clean up out of thread safe scope to prevent dead lock due to hierarchy items
            Debug.Assert(item.InUse == true);
            item.CleanUp();
            Debug.Assert(item.InUse == false);

            using (CreateSafeContext())
            {
                ThreadSafeRecycle(item);
            }
        }

        /// <inheritdoc />
        public void Recycle<T>(IReadOnlyCollection<T> items)
            where T : IPoolItem
        {
            // Clean up out of thread safe scope to prevent dead lock due to hierarchy items
            foreach (var item in items)
            {
                Debug.Assert(item.InUse == true);
                item.CleanUp();
                Debug.Assert(item.InUse == false);
            }

            using (CreateSafeContext())
            {
                foreach (var item in items)
                    ThreadSafeRecycle(item);
            }
        }

        #region Tools        

        /// <summary>
        /// Clears this instance.
        /// </summary>
        protected void Clear()
        {
            IReadOnlyCollection<TItem> items;

            using (CreateSafeContext())
            {
                items = this._items.ToArray();
                this._items.Clear();
            }

            foreach (var item in items)
                item.Dispose();
        }

        /// <summary>
        /// Creates a thread safe context.
        /// </summary>
        protected abstract IDisposable CreateSafeContext();

        /// <summary>
        /// Threads the safe recycle.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThreadSafeRecycle<T>(T item)
            where T : IPoolItem
        {
            if (item is TItem castItem == false)
                throw new InvalidCastException("Couldn't recyle item type " + typeof(T) + " into a pool" + typeof(IPool<TItem>));

            if (this._items.Count < this.MaxRemainItems)
            {
                if (castItem is null)
                    Debug.WriteLine("");

                if (castItem is not null)
                {
                    this._items.Enqueue(castItem);
                    return;
                }
            }

            Debug.Assert(item.InUse == true);
            castItem?.Dispose();
            Debug.Assert(item.InUse == null);
        }

        #endregion
    }
}
