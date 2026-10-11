// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Tasks
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="TaskCompletionSource" />
    /// <seealso cref="ITaskCompletionSourceEx" />
    public sealed class TaskCompletionSourceEx : TaskCompletionSource, ITaskCompletionSourceEx
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskCompletionSourceEx"/> class.
        /// </summary>
        public TaskCompletionSourceEx(object? state = null)
        {
            this.State = state;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public Type ExpectedResultType
        {
            get { return NoneType.Trait; }
        }

        /// <inheritdoc />
        public object? State { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task GetTask()
        {
            return base.Task;
        }

        /// <inheritdoc />
        public void SetResultObject<TGenericResult>(in TGenericResult? _ = default)
        {
            base.SetResult();
        }

        /// <inheritdoc />
        public bool TrySetResultObject<TGenericResult>(in TGenericResult? _ = default)
        {
            return base.TrySetResult();
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <seealso cref="TaskCompletionSource" />
    /// <seealso cref="ITaskCompletionSourceEx" />
    public sealed class TaskCompletionSourceEx<TResult> : TaskCompletionSource<TResult>, ITaskCompletionSourceEx
    {
        #region Fields

        private static readonly Type s_resultTraits;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="TaskCompletionSourceEx{TResult}"/> class.
        /// </summary>
        static TaskCompletionSourceEx()
        {
            s_resultTraits = typeof(TResult);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskCompletionSourceEx{TResult}"/> class.
        /// </summary>
        public TaskCompletionSourceEx(object? state)
        {
            this.State = state;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public Type ExpectedResultType
        {
            get { return s_resultTraits; }
        }

        /// <inheritdoc />
        public object? State { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public Task GetTask()
        {
            return base.Task;
        }

        /// <inheritdoc />
        public void SetResultObject<TGenericResult>(in TGenericResult? obj = default)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            if (object.ReferenceEquals(obj, default))
            {
                base.SetResult(default);
                return;
            }
            else if (obj is TResult result)
            {
                base.SetResult(result);
                return;
            }

            throw new InvalidCastException("Generic result " + obj + " must be a " + typeof(TResult));
#pragma warning restore CS8604 // Possible null reference argument.
        }

        /// <inheritdoc />
        public bool TrySetResultObject<TGenericResult>(in TGenericResult? obj = default)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            if (object.ReferenceEquals(obj, default))
                return base.TrySetResult(default);
            else if (obj is TResult result)
                return base.TrySetResult(result);

            throw new InvalidCastException("Generic result " + obj + " must be a " + typeof(TResult));
#pragma warning restore CS8604 // Possible null reference argument.
        }

        #endregion
    }
}
