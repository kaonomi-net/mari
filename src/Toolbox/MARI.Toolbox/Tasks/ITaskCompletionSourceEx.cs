// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Factorised interface of <see cref="TaskCompletionSource"/> && <see cref="TaskCompletionSource{TResult}"/>
    /// </summary>
    public interface ITaskCompletionSourceEx
    {
        #region Properties

        /// <summary>
        /// Gets the expected type of the result.
        /// </summary>
        /// <remarks>
        ///     <see cref="NoneType"/> if no type is expected
        /// </remarks>
        Type ExpectedResultType { get; }

        /// <summary>
        /// Gets information store at the creation
        /// </summary>
        object? State { get; }

        #endregion

        #region Methods

        /// <inheritdoc cref="TaskCompletionSource.Task" />
        Task GetTask();

        /// <inheritdoc cref="TaskCompletionSource.SetCanceled" />
        public void SetCanceled();

        /// <inheritdoc cref="TaskCompletionSource.SetCanceled(CancellationToken)" />
        public void SetCanceled(CancellationToken cancellationToken);

        /// <inheritdoc cref="TaskCompletionSource.SetException(IEnumerable{Exception})" />
        public void SetException(IEnumerable<Exception> exceptions);

        /// <inheritdoc cref="TaskCompletionSource.SetException(Exception)" />
        public void SetException(Exception exception);

        /// <inheritdoc cref="TaskCompletionSource.TrySetCanceled" />
        public bool TrySetCanceled();

        /// <inheritdoc cref="TaskCompletionSource.TrySetCanceled(CancellationToken)" />
        public bool TrySetCanceled(CancellationToken cancellationToken);

        /// <inheritdoc cref="TaskCompletionSource.TrySetException(IEnumerable{Exception})" />
        public bool TrySetException(IEnumerable<Exception> exceptions);

        /// <inheritdoc cref="TaskCompletionSource.TrySetException(Exception)" />
        public bool TrySetException(Exception exception);

        /// <inheritdoc cref="TaskCompletionSource{TResult}.TrySetResult(TResult)" />
        /// <param name="obj">Result of the task, null or empty if no result is expected.</param>
        /// <remarks>
        ///     The result type MUST strictly correct
        /// </remarks>
        public bool TrySetResultObject<TGenericResult>(in TGenericResult? obj = default);

        /// <inheritdoc cref="TaskCompletionSource{TResult}.SetResult(TResult)" />
        /// <param name="obj">Result of the task, null or empty if no result is expected.</param>
        /// <remarks>
        ///     The result type MUST strictly correct
        /// </remarks>
        public void SetResultObject<TGenericResult>(in TGenericResult? obj = default);

        #endregion
    }
}
