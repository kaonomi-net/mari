// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.Commands
{
    using MARI.Toolbox.Abstractions.Commands;

    using System;

    /// <summary>
    /// Relay action to callback
    /// </summary>
    /// <seealso cref="ICommandExt" />
    public class DelegateCommand : ICommandExt
    {
        #region Fields

        private readonly Predicate<object?>? _predicate;
        private readonly Action _callback;

        private long _running;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class.
        /// </summary>
        public DelegateCommand(Action callback,
                               Predicate<object?>? predicate = null)
        {
            this._callback = callback;
            this._predicate = predicate;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public bool IsRunning
        {
            get { return Interlocked.Read(ref this._running) > 0; }
        }

        #endregion

        #region Event

        /// <inheritdoc />
        public event EventHandler? CanExecuteChanged;

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool CanExecute(object? parameter)
        {
            return this._predicate?.Invoke(parameter) ?? true;
        }

        /// <inheritdoc />
        public virtual void Execute(object? parameter)
        {
            StartRunningScope();
            try
            {
                this._callback();
            }
            finally
            {
                StopRunningScope();
            }
        }

        /// <summary>
        /// Starts the running scope.
        /// </summary>
        protected void StartRunningScope()
        {
            Interlocked.Increment(ref this._running);
        }

        /// <summary>
        /// Stops the running scope.
        /// </summary>
        protected void StopRunningScope()
        {
            Interlocked.Decrement(ref this._running);
        }

        /// <inheritdoc />
        public void RefreshCanExecuteStatus()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }

    /// <summary>
    /// Relay action to callback
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <seealso cref="ICommandExt" />
    public class DelegateCommand<TState> : DelegateCommand, ICommandExt<TState>
    {
        #region Fields

        private readonly Action<TState?> _stateCallback;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand{TState}"/> class.
        /// </summary>
        public DelegateCommand(Action<TState?> callback,
                               Predicate<object?>? predicate = null)
            : base(() => throw new NotSupportedException(), predicate)
        {
            this._stateCallback = callback;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Execute(object? parameter)
        {
            StartRunningScope();
            try
            {
                this._stateCallback((TState?)parameter);
            }
            finally
            {
                StopRunningScope();
            }
        }

        #endregion
    }
}
