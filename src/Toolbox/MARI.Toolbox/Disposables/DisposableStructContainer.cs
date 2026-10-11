// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Disposables
{
    using MARI.Toolbox.Abstractions.Disposables;

    /// <summary>
    /// Attention dispose only on manual call
    /// </summary>
    /// <typeparam name="TContent">The type of the content.</typeparam>
    public struct DisposableStructContainer<TContent> : ISafeDisposable<TContent>, IDisposable
    {
        #region Fields
        
        private readonly bool _disposeContent;
        private long _disposeCounter;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DisposableContainer{TContent}"/> class.
        /// </summary>
        public DisposableStructContainer(TContent content, bool disposeContent = false) 
        {
            this.Content = content;
            this._disposeContent = disposeContent;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public TContent Content { get; }

        /// <inheritdoc />
        public bool IsDisposed
        {
            get { return Interlocked.Read(ref this._disposeCounter) > 0; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.Increment(ref _disposeCounter) == 1)
                return;

            if (this._disposeContent && this.Content is IDisposable disposable)
                disposable.Dispose();
        }

        #endregion
    }
}
