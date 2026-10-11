// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.UnitTests.ToolKit.Services
{
    using MARI.Framework.Core.Abstractions.Diagnostics;

    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Log consumer used to track execution flow during unit test
    /// </summary>
    /// <remarks>
    ///     Must be setup as a remote services
    /// </remarks>
    /// <seealso cref="IDiagnosticLogConsumer" />
    public sealed class TestDiagnosticLogConsumer : IDiagnosticLogConsumer
    {
        #region Fields

        private static readonly TimeSpan s_safetyDelay = TimeSpan.FromSeconds(0.5);

        private readonly ConcurrentBag<IDiagnosticLog> _logs;
        private long _logCounter;
        private TaskCompletionSource _logReceivedSignal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TestDiagnosticLogConsumer"/> class.
        /// </summary>
        public TestDiagnosticLogConsumer()
        {
            this._logs = new ConcurrentBag<IDiagnosticLog>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the logs.
        /// </summary>
        public IReadOnlyCollection<IDiagnosticLog> Logs
        {
            get { return this._logs; }
        }

        #endregion

        #region Methods


        /// <summary>
        /// Store log for future analyses
        /// </summary>
        public Task LogAsync<TLog>(TLog log, CancellationToken token)
            where TLog : class, IDiagnosticLog
        {
            this._logs.Add(log);
            Interlocked.Increment(ref this._logCounter);

            // Wake up every pending WaitLogsAsync and arm a new signal for the next log
            Interlocked.Exchange(ref this._logReceivedSignal, new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously))
                       .TrySetResult();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Wait (deterministically) until at least <paramref name="expectedLogCount"/> logs have been received.
        /// </summary>
        /// <exception cref="TimeoutException">Raised if the expected count is not reached before <paramref name="timeout"/></exception>
        public Task FlushAsync(long expectedLogCount, TimeSpan timeout, CancellationToken token = default)
        {
            return WaitLogsAsync(_ => Interlocked.Read(ref this._logCounter) >= expectedLogCount, timeout, token);
        }

        /// <summary>
        /// Wait (deterministically) until the received logs match <paramref name="endOfFlowCondition"/>
        /// (expected end-of-flow log, expected count, ...).
        /// </summary>
        /// <exception cref="TimeoutException">Raised if the condition is not reached before <paramref name="timeout"/></exception>
        public async Task WaitLogsAsync(Func<IReadOnlyCollection<IDiagnosticLog>, bool> endOfFlowCondition, TimeSpan timeout, CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(endOfFlowCondition);

            using (var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                timeoutSource.CancelAfter(timeout);

                while (true)
                {
                    // Capture the signal BEFORE checking the condition to never miss a log received in between
                    var signal = Volatile.Read(ref this._logReceivedSignal);

                    if (endOfFlowCondition(this._logs))
                        return;

                    try
                    {
                        await signal.Task.WaitAsync(timeoutSource.Token);
                    }
                    catch (OperationCanceledException) when (!token.IsCancellationRequested)
                    {
                        if (endOfFlowCondition(this._logs))
                            return;

                        throw new TimeoutException("Expected diagnostic logs not received after " + timeout + " (received " + Interlocked.Read(ref this._logCounter) + ")");
                    }
                }
            }
        }

        /// <summary>
        /// Ensure all log have been received
        /// </summary>
        /// <remarks>
        ///     Heuristic : use time between last log received.
        ///     Prefer <see cref="FlushAsync(long, TimeSpan, CancellationToken)"/> or <see cref="WaitLogsAsync"/> that wait for an explicit end-of-flow condition.
        /// </remarks>
        public async Task FlushAsync(CancellationToken token)
        {
            long logCount;
            long currentCount;
            do
            {
                logCount = Interlocked.Read(ref this._logCounter);

                await Task.Delay(s_safetyDelay, token);
                token.ThrowIfCancellationRequested();

                currentCount = Interlocked.Read(ref this._logCounter);

                Debug.WriteLine("-- logCount : {0} - currentCount - {1}", logCount, currentCount);

                // Continue if during s_safetyDelay other log arrived
            } while (logCount != currentCount);

            // Unlock if no log have been founded during s_safetyDelay
        }

        #endregion
    }
}
