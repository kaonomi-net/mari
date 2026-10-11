// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF
{
    using MARI.Toolbox.Abstractions.Disposables;
    using MARI.Toolbox.Abstractions.Proxies;
    using MARI.Toolbox.Disposables;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Disposable <see cref="BaseViewModel"/>
    /// </summary>
    /// <seealso cref=".BaseViewModel" />
    /// <seealso cref="ISafeDisposable" />
    public abstract class DisposableBaseViewModel : BaseViewModel, ISafeDisposable
    {
        #region Fields

        private long _disposableCount;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DisposableBaseViewModel"/> class.
        /// </summary>
        /// <param name="dispatcherProxy"></param>
        protected DisposableBaseViewModel(IDispatcherProxy dispatcherProxy)
            : base(dispatcherProxy)
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="DisposableBaseViewModel"/> class.
        /// </summary>
        ~DisposableBaseViewModel()
        {
            Dispose(true);
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool IsDisposed
        {
            get { return Interlocked.Read(ref this._disposableCount) > 0; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(false);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        private void Dispose(bool fromFinalizer)
        {
            if (Interlocked.Increment(ref this._disposableCount) > 1)
                return;

            // On the finalizer thread managed objects could already be finalized: only release unmanaged resources
            if (fromFinalizer)
            {
                DisposeUnmanaged();
                return;
            }

            DisposeBegin();

            DisposeAllResources();

            DisposeManaged();

            DisposeUnmanaged();

            DisposeEnd();
        }

        /// <inheritdoc cref="SafeDisposable.DisposeBegin"/>
        protected virtual void DisposeBegin()
        {
        }

        /// <inheritdoc cref="SafeDisposable.DisposeUnmanaged"/>
        protected virtual void DisposeUnmanaged()
        {
        }

        /// <inheritdoc cref="SafeDisposable.DisposeManaged"/>
        protected virtual void DisposeManaged()
        {
        }

        /// <inheritdoc cref="SafeDisposable.DisposeEnd"/>
        protected virtual void DisposeEnd()
        {
        }

        #endregion
    }
}
