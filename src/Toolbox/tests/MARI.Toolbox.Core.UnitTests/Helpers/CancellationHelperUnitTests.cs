// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.UnitTests.Helpers
{
    using MARI.Toolbox.Helpers;

    using NFluent;

    using System;
    using System.Diagnostics;

    /// <summary>
    /// Test <see cref="CancellationHelper"/> functionalities
    /// </summary>
    public sealed class CancellationHelperUnitTests
    {
        /// <summary>
        /// Test that <see cref="CancellationHelper.SingleAccessScope"/> allow only one scope to run at same time to automatically cancel the previous one
        /// </summary>
        [Fact]
        public void CancellationHelper_SingleScopeAccess()
        {
            CancellationTokenSource? source = null;

            var callCount = 0;

            using (var locker = new SemaphoreSlim(1))
            {
                var getAccessor = new Func<CancellationTokenSource?>(() =>
                {
                    // Ensure that locker is lock
                    Check.That(locker.CurrentCount).IsEqualTo(0);
                    callCount++;

                    return source;
                });

                var setAccessor = new Action<CancellationTokenSource>((s) =>
                {
                    // Ensure that locker is lock
                    Check.That(locker.CurrentCount).IsEqualTo(0);

                    callCount++;
                    source = s;
                });

                using (var scope = CancellationHelper.SingleAccessScope(locker, getAccessor, setAccessor))
                {
                    using (var secondScope = CancellationHelper.SingleAccessScope(locker, getAccessor, setAccessor))
                    {
                        // Check that is a second scope is open on the same code section the first one is cancelled
                        Check.That(scope.Content.IsCancellationRequested).IsTrue();
                        Check.That(secondScope.Content.IsCancellationRequested).IsFalse();

                        Check.That(scope.Content).Not.IsSameReferenceAs(secondScope.Content);
                    }
                }

                Check.That(callCount).IsEqualTo(4);
            }
        }

        /// <summary>
        /// Test that <see cref="CancellationHelper.SingleAccessScope"/> allow only one scope to run at same time to automatically cancel the previous one
        /// USING a <see cref="CancellationContext"/>
        /// </summary>
        [Fact]
        public void CancellationHelper_And_CancellationContext_SingleScopeAccess()
        {
            using (var locker = new SemaphoreSlim(1))
            {
                using (var ctx = new CancellationContext())
                {
                    using (var scope = ctx.Lock())
                    {
                        using (var secondScope = ctx.Lock())
                        {
                            // Check that is a second scope is open on the same code section the first one is cancelled
                            Check.That(scope.Content.IsCancellationRequested).IsTrue();
                            Check.That(secondScope.Content.IsCancellationRequested).IsFalse();

                            Check.That(scope.Content).Not.IsSameReferenceAs(secondScope.Content);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Test <see cref="CancellationHelper.Timeout(TimeSpan?)"/> correcly cancel the token after the time asked
        /// </summary>
        /// <remarks>
        ///     Only machine-independent bounds are checked : the token is never cancelled BEFORE the requested delay
        ///     (minus the OS timer resolution) and it IS cancelled eventually (wide bounded wait, no fixed upper wall-clock bound).
        /// </remarks>
        [Fact] // Security test MUST exist normally on the time setups
        public async Task CancellationHelper_Timeout()
        {
            var requestedTimeout = TimeSpan.FromMilliseconds(300);

            var stopwatch = Stopwatch.StartNew();
            var token = CancellationHelper.Timeout(requestedTimeout);

            var cancelled = new TaskCompletionSource<TimeSpan>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (token.Register(() => cancelled.TrySetResult(stopwatch.Elapsed)))
            {
                // Bounded wait : fails instead of hanging if the token is never cancelled
                var cancelledAfter = await cancelled.Task.WaitAsync(TimeSpan.FromSeconds(30));

                Check.That(token.IsCancellationRequested).IsTrue();

                // Timer resolution tolerance (Windows default timer tick ~15.6 ms)
                Check.That(cancelledAfter.TotalMilliseconds).IsGreaterOrEqualThan((requestedTimeout - TimeSpan.FromMilliseconds(50)).TotalMilliseconds);
            }

            Check.ThatAsyncCode(async () => await Task.Delay(Timeout.Infinite, token)).Throws<TaskCanceledException>();
        }
    }
}
