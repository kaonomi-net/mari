// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Helpers
{
    using MARI.Toolbox.Abstractions.Disposables;
    using MARI.Toolbox.Disposables;

    using System;

    /// <summary>
    /// Help cencallation algorithme
    /// </summary>
    public static class CancellationHelper
    {
        #region Fields

        private static readonly TimeSpan s_defaultScopeTimeout = TimeSpan.FromSeconds(10);

        #endregion

        /// <summary>
        /// Create a timeout <see cref="CancellationToken"/>
        /// </summary>
        /// <remarks>
        ///     The underlying timer-backed <see cref="CancellationTokenSource"/> is never disposed (kept alive until the timeout):
        ///     do not use it in hot paths, prefer <see cref="DisposableTimeout(TimeSpan?)"/> or timeout-based wait APIs.
        /// </remarks>
        public static CancellationToken Timeout(TimeSpan? timeout = null)
        {
            return new CancellationTokenSource(timeout ?? s_defaultScopeTimeout).Token;
        }

        /// <summary>
        /// Create a timeout <see cref="CancellationToken"/>
        /// </summary>
        public static ISafeDisposable<CancellationToken> DisposableTimeout(TimeSpan? timeout = null)
        {
            var source = new CancellationTokenSource(timeout ?? s_defaultScopeTimeout);
            return new DisposableAction<CancellationToken>(_ => source.Dispose(), source.Token);
        }

        /// <summary>
        /// Associate with a using provide a safe scope accesible by only one thread at time
        /// </summary>
        public static ISafeDisposable<CancellationToken> SingleAccessScope(SemaphoreSlim locker,
                                                                           Func<CancellationTokenSource?> getCurrentSource,
                                                                           Action<CancellationTokenSource> setterNewSource,
                                                                           TimeSpan? timeout = null,
                                                                           params CancellationToken[] extraTokens)
        {
#if DEBUG
            ArgumentNullException.ThrowIfNull(locker);
            ArgumentNullException.ThrowIfNull(getCurrentSource);
            ArgumentNullException.ThrowIfNull(setterNewSource);
#endif

            var lockTimeout = timeout ?? s_defaultScopeTimeout;
            if (!locker.Wait(lockTimeout))
                throw new TimeoutException("Failed to acquired single access scope lock in " + lockTimeout);

            try
            {
                var current = getCurrentSource();
                current?.Cancel();

                if (extraTokens.Any())
                    current = CancellationTokenSource.CreateLinkedTokenSource(extraTokens);
                else
                    current = new CancellationTokenSource();
                setterNewSource(current);

                return new DisposableAction<CancellationToken>(static _ => { }, current.Token);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                locker.Release();
            }
        }
    }
}
