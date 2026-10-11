// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Disposables
{
    using System;

    /// <summary>
    /// Disposable object that perform an action pass in argument at dispose time
    /// </summary>
    public class DisposableAsyncAction<TContent> : SafeAsyncDisposable<TContent>
    {
        #region Fields

        private readonly Func<TContent, ValueTask> _callbackAtDisposeTime;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DisposableAsyncAction"/> class.
        /// </summary>
        public DisposableAsyncAction(Func<TContent, ValueTask> callbackAtDisposeTime, TContent content)
            : base(content)
        {
            ArgumentNullException.ThrowIfNull(callbackAtDisposeTime);
            this._callbackAtDisposeTime = callbackAtDisposeTime;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override async ValueTask DisposeBeginAsync()
        {
            await this._callbackAtDisposeTime(this.Content);
            await base.DisposeBeginAsync();
        }

        #endregion
    }

    /// <summary>
    /// Disposable object that perform an action pass in argument at dispose time
    /// </summary>
    public class DisposableAsyncAction : DisposableAsyncAction<NoneType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisposableAsyncAction"/> class.
        /// </summary>
        public DisposableAsyncAction(Func<ValueTask> callbackAtDisposeTime)
            : base(_ => callbackAtDisposeTime(), NoneType.Instance)
        {
        }
    }
}