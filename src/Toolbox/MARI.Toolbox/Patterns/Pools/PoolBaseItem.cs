// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Pools
{
    using MARI.Toolbox.Abstractions.Patterns.Pools;
    using MARI.Toolbox.Disposables;

    /// <summary>
    /// Base class of a pool item
    /// </summary>
    /// <seealso cref="SafeDisposable" />
    /// <seealso cref="IPoolItem" />
    public abstract class PoolBaseItem : SafeDisposable, IPoolItem
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="PoolBaseItem"/> class.
        /// </summary>
        protected PoolBaseItem()
        {
            this.InUse = false; 
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool? InUse { get; private set; }

        /// <summary>
        /// Gets the pool source.
        /// </summary>
        protected IPool? PoolSource { get; private set; }

        #endregion

        /// <inheritdoc />
        public void CleanUp()
        {
            this.InUse = false;
            OnCleanUp();
        }

        /// <summary>
        /// Called when [clean up].
        /// </summary>
        protected abstract void OnCleanUp();

        /// <inheritdoc />
        public void Prepare(IPool sourcePool)
        {
            this.PoolSource = sourcePool;
            this.InUse = true;

            OnPrepare();
        }

        /// <summary>
        /// Called when [prepare].
        /// </summary>
        protected virtual void OnPrepare()
        {
        }

        /// <inheritdoc />
        public void Release()
        {
            var poolSource = this.PoolSource;
            this.PoolSource = null;
            poolSource?.Recycle(this);
        }

        /// <inheritdoc />
        protected sealed override void DisposeBegin()
        {
            this.InUse = null;
            base.DisposeBegin();
        }
    }
}
