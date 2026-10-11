// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Disposables
{
    using MARI.Toolbox.Abstractions.Disposables;
    using MARI.Toolbox.Memories;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Thread Safe disposable implementations
    /// </summary>
    public abstract class SafeAsyncDisposable : IAsyncDisposable
    {
        #region Fields

        private readonly SafeContainer<IAsyncDisposable> _asyncDisposableDependencies;
        private readonly SafeContainer<IDisposable> _disposableDependencies;
        private long _disposableCounter = 0;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeAsyncDisposable"/> class.
        /// </summary>
        public SafeAsyncDisposable()
        {
            this._asyncDisposableDependencies = new SafeContainer<IAsyncDisposable>();
            this._disposableDependencies = new SafeContainer<IDisposable>();
        }

        // No finalizer: blocking the finalizer thread on async dispose stalls the whole process finalization
        // and the dispose chain touches managed objects that may already be finalized.

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed
        {
            get { return Interlocked.Read(ref this._disposableCounter) > 0; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            await DisposeAsync(true);
        }

        /// <summary>
        /// Registers the disposable dependency to dispose when this instance is.
        /// </summary>
        protected Guid? RegisterDisposableDependency<TInst>(TInst inst)
        {
            if (inst is IAsyncDisposable asyncDisposable)
                return this._asyncDisposableDependencies.Register(asyncDisposable);
            else if (inst is IDisposable disposable)
                return this._disposableDependencies.Register(disposable);

            return null;
        }

        /// <summary>
        /// Check and Throws if disposed.
        /// </summary>
        protected void CheckAndThrowIfDisposed()
        {
            if (this.IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        #region Tools

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        private async ValueTask DisposeAsync(bool manualCall)
        {
            if (Interlocked.Increment(ref this._disposableCounter) > 1)
                return;

            await DisposeBeginAsync();

            await DisposeUnmanagedAsync();

            if (manualCall)
                await DisposeManagedAsync();

            var dependencyAsyncDisposable = this._asyncDisposableDependencies.GetContainerCopy();
            this._asyncDisposableDependencies.Clear();

            foreach (var dependency in dependencyAsyncDisposable)
                await dependency.DisposeAsync();

            var dependencyDisposable = this._disposableDependencies.GetContainerCopy();
            this._disposableDependencies.Clear();

            foreach (var dependency in dependencyDisposable)
                dependency.Dispose();

            await DisposeEndAsync();
        }

        /// <summary>
        /// Start to disposed
        /// </summary>
        protected virtual ValueTask DisposeBeginAsync()
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Called to disposed un managed resources
        /// </summary>
        protected virtual ValueTask DisposeUnmanagedAsync()
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Called to disposed managed resources only on manual call
        /// </summary>
        protected virtual ValueTask DisposeManagedAsync()
        {
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Call at the end of the dispose process
        /// </summary>
        protected virtual ValueTask DisposeEndAsync()
        {
            return ValueTask.CompletedTask;
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Thread Safe disposable implementations
    /// </summary>
    public abstract class SafeAsyncDisposable<TContent> : SafeAsyncDisposable, ISafeAsyncDisposable<TContent>, ISafeAsyncDisposable, IAsyncDisposable
    {
        #region Fields

        private readonly bool _disposeContent;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeAsyncDisposable"/> class.
        /// </summary>
        public SafeAsyncDisposable(TContent content, bool disposeContent = false)
        {
            this.Content = content;
            this._disposeContent = disposeContent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the content.
        /// </summary>
        public TContent Content { get; }

        #endregion

        #region Methods

        protected override async ValueTask DisposeBeginAsync()
        {
            if (this._disposeContent && this.Content is IAsyncDisposable disposable)
                await disposable.DisposeAsync();

            await base.DisposeBeginAsync();
        }

        #endregion
    }
}
