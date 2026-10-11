// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Supports
{
    using MARI.Toolbox.Abstractions.Supports;
    using MARI.Toolbox.Disposables;

    using System.Threading.Tasks;

    /// <summary>
    /// Base class to support initialization without state input
    /// </summary>
    /// <seealso cref="SupportBaseInternalInitialization" />
    /// <seealso cref="ISupportInitialization" />
    public abstract class SupportBaseInitialization : SafeDisposable, ISupportInitialization
    {
        #region Fields
        
        private readonly SupportInitializationImplementation<NoneType> _impl;
        
        #endregion

        #region Ctor

        protected SupportBaseInitialization()
        {
            this._impl = new SupportInitializationImplementation<NoneType>((_, token) => OnInitializedAsync(token));
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool IsInitializing
        {
            get { return this._impl.IsInitializing; }
        }

        /// <inheritdoc />
        public bool IsInitialized
        {
            get { return this._impl.IsInitialized; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask InitializationAsync(CancellationToken token = default)
        {
            return this._impl.InitializationAsync(token);
        }

        /// <summary>
        /// Initialization without state
        /// </summary>
        protected abstract ValueTask OnInitializedAsync(CancellationToken token);

        /// <inheritdoc />
        protected override void DisposeBegin()
        {
            try
            {
                this._impl?.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
            base.DisposeBegin();
        }

        #endregion
    }

    /// <summary>
    /// Base Implementation of <see cref="ISupportInitialization"/>
    /// </summary>
    /// <seealso cref="ISupportInitialization" />
    public abstract class SupportBaseInitialization<TState> : SafeDisposable, ISupportInitialization<TState>
    {
        #region Fields

        private readonly SupportInitializationImplementation<TState> _impl;

        #endregion

        #region Ctor

        protected SupportBaseInitialization()
        {
            this._impl = new SupportInitializationImplementation<TState>((state, token) => OnInitializingAsync(state, token));
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool IsInitializing
        {
            get { return this._impl.IsInitializing; }
        }

        /// <inheritdoc />
        public bool IsInitialized
        {
            get { return this._impl.IsInitialized; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask InitializationAsync(CancellationToken token = default)
        {
            return this._impl.InitializationAsync(token);
        }

        /// <inheritdoc />
        public ValueTask InitializationAsync(TState? initializationState = default, CancellationToken token = default)
        {
            return this._impl.InitializationAsync(initializationState, token);
        }

        /// <inheritdoc cref="ISupportInitialization.InitializationAsync{TState}(TState?, CancellationToken)" />
        protected abstract ValueTask OnInitializingAsync(TState? initializationState, CancellationToken token);

        /// <inheritdoc />
        protected override void DisposeBegin()
        {
            try
            {
                this._impl?.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
            base.DisposeBegin();
        }

        #endregion
    }
}
