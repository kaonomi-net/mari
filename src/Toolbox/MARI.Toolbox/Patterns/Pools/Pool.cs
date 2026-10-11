// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Pools
{
    using MARI.Toolbox.Abstractions.Patterns.Pools;
    using MARI.Toolbox.Disposables;

    using System;

    /// <summary>
    /// Class pool NOT Threadsafe
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <seealso cref="PoolBase{TItem}" />
    public sealed class Pool<TItem> : PoolBase<TItem>
        where TItem : class, IPoolItem, new()
    {
        #region Ctors

        /// <summary>
        /// Initializes the <see cref="Pool{TItem}"/> class.
        /// </summary>
        static Pool()
        {
            Default = new Pool<TItem>(20_000, 50);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pool{TItem}"/> class.
        /// </summary>
        public Pool(int maxRemainItems, int? initItems = null) 
            : base(maxRemainItems, initItems)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static Pool<TItem> Default { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override IDisposable CreateSafeContext()
        {
            return SafeDisposable.Empty;
        }

        #endregion
    }
}
