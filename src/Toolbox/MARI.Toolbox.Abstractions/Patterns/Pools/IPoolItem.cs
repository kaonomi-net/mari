// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Patterns.Pools
{
    using MARI.Toolbox.Abstractions.Disposables;

    /// <summary>
    /// Define a reusable item attached to a pool
    /// </summary>
    public interface IPoolItem : ISafeDisposable
    {
        #region Properties

        /// <summary>
        /// Gets the in use. <br />
        /// <c>True</c> if item removed from pool, <br />
        /// <c>Null</c> if the item have been disposed, <br />
        /// <c>False</c> if the item is pending in the pool
        /// </summary>
        bool? InUse { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Prepares this instance to be use.
        /// </summary>
        void Prepare(IPool sourcePool);

        /// <summary>
        /// Releases this instance to returned to the pool.
        /// </summary>
        void Release();

        /// <summary>
        /// Cleans up the content before recycling
        /// </summary>
        void CleanUp();

        #endregion
    }
}
