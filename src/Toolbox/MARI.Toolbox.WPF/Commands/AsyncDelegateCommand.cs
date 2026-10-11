// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.Commands
{
    using MARI.Toolbox.Abstractions.Commands;
    using MARI.Toolbox.Abstractions.Proxies;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Async delegate command
    /// </summary>
    /// <seealso cref="IAsyncCommandExt" />
    public class AsyncDelegateCommand : DelegateCommand, IAsyncCommandExt
    {
        #region Fields

        private readonly Func<ValueTask> _callback;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncDelegateCommand"/> class.
        /// </summary>
        public AsyncDelegateCommand(IDispatcherProxy dispatcherProxy,
                                    Func<ValueTask> callback,
                                    Predicate<object?>? predicate = null)
            : base(() => throw new NotSupportedException(), predicate)
        {
            ArgumentNullException.ThrowIfNull(dispatcherProxy);
            ArgumentNullException.ThrowIfNull(callback);

            this.DispatcherProxy = dispatcherProxy;
            this._callback = callback;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the dispatcher proxy.
        /// </summary>
        public IDispatcherProxy DispatcherProxy { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Execute(object? parameter)
        {
            // Prevent double execution from the UI while a previous one is still running
            if (this.IsRunning)
                return;

            // Mark as running synchronously to close the double execution window
            StartRunningScope();

            // Run the callback in the dispatcher context to let it safely touch UI-bound state
            this.DispatcherProxy.Send(() => _ = ExecuteOnDispatcherAsync(parameter));
        }

        /// <summary>
        /// Executes the callback from the dispatcher context, running scope already started
        /// </summary>
        private async Task ExecuteOnDispatcherAsync(object? parameter)
        {
            try
            {
                RefreshCanExecuteStatus();
                await OnSafeExecuteAsync(parameter);
            }
            catch (Exception ex)
            {
                this.DispatcherProxy.Throw(ex);
            }
            finally
            {
                StopRunningScope();
                RefreshCanExecuteStatus();
            }
        }

        /// <inheritdoc />
        public ValueTask ExecuteAsync()
        {
            return ExecuteImplAsync(null);
        }

        #region Tools

        /// <inheritdoc />
        protected async ValueTask ExecuteImplAsync(object? parameter)
        {
            StartRunningScope();
            try
            {
                await OnSafeExecuteAsync(parameter);
            }
            finally
            {
                StopRunningScope();
            }
        }

        /// <summary>
        /// Called when [safe execute asynchronous].
        /// </summary>
        protected virtual async ValueTask OnSafeExecuteAsync(object? parameter)
        {
            await this._callback();
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Async delegate command
    /// </summary>
    /// <seealso cref="IAsyncCommandExt" />
    public class AsyncDelegateCommand<TState> : AsyncDelegateCommand, IAsyncCommandExt<TState>
    {
        #region Fields

        private readonly Func<TState?, ValueTask> _stateCallback;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncDelegateCommand{TState}"/> class.
        /// </summary>
        public AsyncDelegateCommand(IDispatcherProxy dispatcherProxy,
                                    Func<TState?, ValueTask> callback,
                                    Predicate<object?>? predicate = null)
            : base(dispatcherProxy, () => throw new NotSupportedException(), predicate)
        {
            this._stateCallback = callback;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask ExecuteAsync(TState state)
        {
            return ExecuteImplAsync(state);
        }

        /// <inheritdoc />
        protected override async ValueTask OnSafeExecuteAsync(object? parameter)
        {
            await this._stateCallback((TState?)parameter);
        }

        #endregion
    }
}
