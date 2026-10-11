// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Pools
{
    using MARI.Toolbox.Abstractions.Patterns.Pools;
    using MARI.Toolbox.Extensions;

    using System;

    /// <summary>
    /// Thread Safe pool
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <seealso cref="PoolBase{TItem}" />
    public sealed class ThreadSafePool<TItem> : PoolBase<TItem>, IDisposable
        where TItem : class, IPoolItem, new()
    {
        #region Fields
        
        private readonly SemaphoreSlim _locker;

        #endregion

        #region Ctors

        /// <summary>
        /// Initializes the <see cref="Pool{TItem}"/> class.
        /// </summary>
        static ThreadSafePool()
        {
            Default = new ThreadSafePool<TItem>(20_000, 50);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafePool{TItem}"/> class.
        /// </summary>
        public ThreadSafePool(int maxRemainItems, int? initItems = null)
            : base(maxRemainItems, initItems)
        {
            this._locker = new SemaphoreSlim(1);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ThreadSafePool{TItem}"/> class.
        /// </summary>
        ~ThreadSafePool()
        {
            Dispose(true);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static ThreadSafePool<TItem> Default { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(false);
        }

        /// <inheritdoc />
        protected override IDisposable CreateSafeContext()
        {
            return this._locker.Lock();
        }

        /// <inheritdoc cref="IDisposable.Dispose" />
        private void Dispose(bool _)
        {
            Clear();
            this._locker?.Dispose();
        }

        #endregion
    }
}
