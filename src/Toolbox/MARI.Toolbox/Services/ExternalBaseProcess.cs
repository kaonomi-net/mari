// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Services
{
    using MARI.Toolbox.Abstractions.Services;
    using MARI.Toolbox.Abstractions.Supports;
    using MARI.Toolbox.Disposables;

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Proxy pilot about a external execution process
    /// </summary>
    /// <seealso cref="IExternalProcess" />
    public abstract class ExternalBaseProcess : SafeDisposable, IExternalProcess, ISupportDebugDisplayName
    {
        #region Fields

        private readonly ConcurrentQueue<string> _standardOutputLogger;
        private readonly Subject<string> _standardOutput;

        private readonly ConcurrentQueue<string> _errorOutputLogger;
        private readonly Subject<string> _errorOutput;

        private readonly SemaphoreSlim _locker;
        private Task? _processTask;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalBaseProcess"/> class.
        /// </summary>
        public ExternalBaseProcess(IReadOnlyCollection<string> arguments,
                                   CancellationToken cancellationToken)
        {
            this.CancellationToken = cancellationToken;
            this._locker = new SemaphoreSlim(1);

            this.Arguments = arguments;

            // Standard output
            this._standardOutputLogger = new ConcurrentQueue<string>();
            this._standardOutput = new Subject<string>();
            RegisterDisposableDependency(this._standardOutput);

            var defaultRecordToken = this._standardOutput.Subscribe((n) => this._standardOutputLogger.Enqueue(n));
            RegisterDisposableDependency(defaultRecordToken);

            var standardOutputConnectObservable = this._standardOutput.ObserveOn(TaskPoolScheduler.Default)
                                                                      .Publish();

            var standardOutputConnectToken = standardOutputConnectObservable.Connect();
            RegisterDisposableDependency(standardOutputConnectToken);

            this.StandardOutputObservable = standardOutputConnectObservable;

            // Error output
            this._errorOutputLogger = new ConcurrentQueue<string>();

            this._errorOutput = new Subject<string>();
            RegisterDisposableDependency(this._errorOutput);

            var defaultErrorRecordToken = this._errorOutput.Subscribe((n) => this._errorOutputLogger.Enqueue(n));
            RegisterDisposableDependency(defaultErrorRecordToken);

            var errorOutputConnectObservable = this._errorOutput.ObserveOn(TaskPoolScheduler.Default)
                                                                   .Publish();

            var errorOutputConnectToken = errorOutputConnectObservable.Connect();
            RegisterDisposableDependency(errorOutputConnectToken);

            this.ErrorOutputObservable = errorOutputConnectObservable;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public abstract string Executable { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<string> Arguments { get; }

        /// <inheritdoc />
        public IObservable<string> StandardOutputObservable { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<string> StandardOutput
        {
            get { return this._standardOutputLogger.ToArray(); }
        }

        /// <inheritdoc />
        public IObservable<string> ErrorOutputObservable { get; }

        /// <inheritdoc />
        public IReadOnlyCollection<string> ErrorOutput
        {
            get { return this._errorOutputLogger.ToArray(); }
        }

        /// <inheritdoc />
        public int? ExitCode { get; protected set; }

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        protected CancellationToken CancellationToken { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Start configured process
        /// </summary>
        public async Task RunAsync()
        {
            await this._locker.WaitAsync(this.CancellationToken);
            try
            {
                await OnRunAsync(this.CancellationToken);
            }
            catch (Exception ex)
            {
                ex.Data.Add(this.GetType().Name, ToDebugDisplayName());
                throw;
            }
            finally
            {
                this._locker.Release();
            }
        }

        /// <inheritdoc />
        public Task GetAwaiterTask()
        {
            this._locker.Wait();
            try
            {
                return this._processTask ?? throw new InvalidOperationException("Run process first");
            }
            finally
            {
                this._locker.Release();
            }
        }

        /// <inheritdoc />
        public abstract Task KillAsync(CancellationToken cancellationToken);

        /// <inheritdoc />
        public abstract string ToDebugDisplayName();

        #region Tools

        /// <summary>
        /// Handles the OutputDataReceived event of the Process control.
        /// </summary>
        protected void ProcessOutputDataReceived(string? data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            this._standardOutput.OnNext(data);
        }

        /// <summary>
        /// Handles the ErrorDataReceived event of the Process control.
        /// </summary>
        protected void ProcessErrorDataReceived(string? data)
        {
            if (string.IsNullOrEmpty(data))
                return;

            this._errorOutput.OnNext(data);
        }

        /// <summary>
        /// Sets the process waiting task.
        /// </summary>
        protected void SetProcessWaitingTask(Task task)
        {
            if (this._processTask is not null)
                throw new ArgumentNullException("this Waiting process MUST be null");
            this._processTask = task;
        }

        /// <summary>
        /// Used to kill dependency process
        /// </summary>
        protected override void DisposeBegin()
        {
            this._locker.Wait();
            try
            {
                OnDisposeThreadSafeBegin();
            }
            finally
            {
                this._locker.Release();
            }

            base.DisposeBegin();
        }

        /// <summary>
        /// Called when [dispose thread safe begin].
        /// </summary>
        protected abstract void OnDisposeThreadSafeBegin();

        /// <summary>
        /// Called when [run asynchronous].
        /// </summary>
        protected abstract Task OnRunAsync(CancellationToken cancellationToken);

        #endregion

        #endregion
    }
}
