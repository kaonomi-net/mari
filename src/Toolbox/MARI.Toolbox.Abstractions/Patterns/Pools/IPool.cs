// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Patterns.Pools
{
    /// <summary>
    /// Define an item pool recyclable
    /// </summary>
    public interface IPool
    {
        #region Properties

        /// <summary>
        /// Gets the maximum remain items.
        /// </summary>
        int MaxRemainItems { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Recycles the specified item.
        /// </summary>
        void Recycle<T>(T item) where T : IPoolItem;

        /// <summary>
        /// Recycles the specified item.
        /// </summary>
        void Recycle<T>(IReadOnlyCollection<T> items) where T : IPoolItem;

        #endregion
    }

    /// <summary>
    /// Define an item pool recyclable
    /// </summary>
    public interface IPool<TItem> : IPool
        where TItem : IPoolItem, new()
    {
        #region Methods

        /// <summary>
        /// Gets an item.
        /// </summary>
        TItem GetItem();

        /// <summary>
        /// Gets <paramref name="count"/> items.
        /// </summary>
        IReadOnlyCollection<TItem> GetItems(ushort count);

        #endregion
    }
}
