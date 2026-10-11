// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Helpers
{
    using MARI.Toolbox.Abstractions.Patterns.Pools;
    using MARI.Toolbox.Patterns.Pools;

    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;

    public static class BindingHelper
    {
        #region Fields

        private readonly static IPool<DPOGetValue> s_dpoGetValue;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BindingHelper" /> class.
        /// </summary>
        static BindingHelper()
        {
            s_dpoGetValue = new ThreadSafePool<DPOGetValue>(10_000, 10);
        }

        #endregion

        #region Nested

        private sealed class DPOGetValue : FrameworkElement, IPoolItem
        {
            #region Fields

            public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value),
                                                                                                  typeof(object),
                                                                                                  typeof(DPOGetValue),
                                                                                                  new PropertyMetadata(null));

            private long _disposed;
            private IPool? _pool;

            #endregion

            #region Properties

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            public object Value
            {
                get { return GetValue(ValueProperty); }
                set { SetValue(ValueProperty, value); }
            }

            /// <inheritdoc />
            public bool? InUse { get; private set; }

            /// <inheritdoc />
            public bool IsDisposed
            {
                get { return Interlocked.Read(ref this._disposed) > 0; }
            }

            #endregion

            #region Methods

            /// <inheritdoc />
            public void CleanUp()
            {
                this.InUse = false;
                BindingOperations.ClearAllBindings(this);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                this.InUse = null;
                CleanUp();
                GC.SuppressFinalize(this);
                Interlocked.Increment(ref this._disposed);
            }

            /// <inheritdoc />
            public void Prepare(IPool sourcePool)
            {
                this._pool = sourcePool;
                this.InUse = true;
            }

            /// <inheritdoc />
            public void Release()
            {
                this._pool?.Recycle(this);
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the binding result.
        /// </summary>
        public static object? GetBindingResult(BindingBase binding, object dataContext)
        {
            var item = s_dpoGetValue.GetItems(1).Single();
            try
            {
                item.DataContext = dataContext;
                BindingOperations.SetBinding(item, DPOGetValue.ValueProperty, binding);
                return item.Value;
            }
            finally
            {
                item.Release();
            }
        }

        #endregion
    }
}
