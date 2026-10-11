// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Supports
{
    using MARI.Toolbox.Abstractions.Supports;
    using MARI.Toolbox.Disposables;

    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implementation of <see cref="ISupportInitialization{TState}"/> using a simple init method
    /// </summary>
    public sealed class SupportInitializationImplementation<TState> : SafeDisposable, ISupportInitialization<TState>
    {
        #region Fields

        private readonly Func<TState?, CancellationToken, ValueTask> _initMethod;

        private TaskCompletionSource _initializingTask;
        private long _initializing;
        private long _initialized;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportBaseInitialization"/> class.
        /// </summary>
        public SupportInitializationImplementation(Func<TState?, CancellationToken, ValueTask> initMethod)
        {
            this._initializingTask = new TaskCompletionSource();
            this._initMethod = initMethod;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool IsInitializing
        {
            get { return Interlocked.Read(ref this._initializing) > 0; }
        }

        /// <inheritdoc />
        public bool IsInitialized
        {
            get { return Interlocked.Read(ref this._initialized) > 0; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask InitializationAsync(CancellationToken token = default)
        {
            return InitializationAsync(default, token);
        }

        /// <summary>
        /// Internal implementation that support state injection
        /// </summary>
        public async ValueTask InitializationAsync(TState? initializationState, CancellationToken token = default)
        {
            var initTask = this._initializingTask.Task;
            if (this.IsInitialized)
                return;

            if (Interlocked.Increment(ref this._initializing) > 1)
            {
                await initTask;
                return;
            }

            try
            {
                try
                {
                    await this._initMethod(initializationState, token);
                    Interlocked.Increment(ref this._initialized);

                    var tmpTask = this._initializingTask;

                    this._initializingTask = new TaskCompletionSource();
                    tmpTask.TrySetResult();
                }
                finally
                {
                    Interlocked.Exchange(ref this._initializing, 0);
                }
            }
            catch (Exception ex)
            {
                this._initializingTask.TrySetException(ex);
                throw;
            }
        }

        #endregion
    }
}
