// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Services
{
    using MARI.Toolbox.Abstractions.Proxies;

    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Threading;

    /// <summary>
    /// Dispatcher on UI dispatcher
    /// </summary>
    /// <seealso cref="IDispatcherProxy" />
    public sealed class UIDispatcher : IDispatcherProxy
    {
        #region Fields

        private readonly Dispatcher _dispatcher;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="UIDispatcher"/> class.
        /// </summary>
        public UIDispatcher(Dispatcher dispatcher)
        {
            this._dispatcher = dispatcher;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance run in the dispatcher thread.
        /// </summary>
        public bool IsCurrentThread
        {
            get { return this._dispatcher.Thread == Thread.CurrentThread; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Send(Action callback)
        {
            if (this.IsCurrentThread)
                callback();
            else
                this._dispatcher.InvokeAsync(callback);
        }

        /// <inheritdoc />
        public void SendAndWait(Action callback)
        {
            if (this.IsCurrentThread)
                callback();
            else
                this._dispatcher.Invoke(callback);
        }

        /// <inheritdoc />
        public async ValueTask SendAsync(Action callback)
        {
            if (this.IsCurrentThread)
                callback();
            else
                await this._dispatcher.InvokeAsync(callback);
        }

        /// <inheritdoc />
        public void Throw(Exception ex, [CallerMemberName] string? callerMemberName = null)
        {
            Send(() => throw new Exception("Exception raised by " + callerMemberName + "\n StackTrace : " + ex.StackTrace, ex));
        }

        #endregion
    }
}
