// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using MARI.Framework.Core.Abstractions;

    using MARI.Toolbox.Abstractions.Attributes;
    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Base class about cluster interaction node/client
    /// </summary>
    /// <typeparam name="TConfig">The type of the configuration.</typeparam>
    /// <seealso cref="SafeAsyncDisposable" />
    public abstract class MARIBasePart<TConfig> : SafeAsyncDisposable, IHostedService
    {
        #region Fields

        private readonly IMARIExecutionHandler _mariExecutionHandler;

        private readonly SemaphoreSlim _locker;
        private readonly bool _hostOwned;
        private readonly ILogger _logger;
        private readonly IHost _host;

        private CancellationTokenSource? _runningTaskCancellationTokenSource;
        private CancellationTokenSource? _runningLinkedTokenSource;
        private Task? _runningTask;
        private bool _hostStopped;
        private bool _isRunning;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterBasePart{TConfig}"/> class.
        /// </summary>
        protected MARIBasePart(IHost host,
                                    TConfig? config,
                                    IMARIExecutionHandler? mariExecutionHandler,
                                    bool hostOwner)
        {
            ArgumentNullException.ThrowIfNull(host);
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(mariExecutionHandler);

            this.Config = config;
            this._host = host;

            this._hostOwned = hostOwner;

            this._logger = this._host.Services.GetService<ILoggerFactory>()?.CreateLogger(GetType().Name) ?? NullLogger.Instance;

            this._mariExecutionHandler = mariExecutionHandler;

            this._locker = new SemaphoreSlim(1);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public TConfig? Config { get; }

        /// <summary>
        /// Gets the services.
        /// </summary>
        protected IServiceProvider Services
        {
            get { return this._host.Services; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is running.
        /// </summary>
        public bool IsRunning
        {
            get { return this._isRunning; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Start the <see cref="ClusterNode"/> and await until the server stop
        /// </summary>
        public async Task StartUntilEndAsync(Func<IServiceProvider, IMARIExecutionHandler, CancellationToken, Task>? runningFunction = null, CancellationToken token = default)
        {
            await StartAsync(runningFunction, token);

            CancellationTokenSource? linkedTokenSource = null;
            try
            {
                try
                {
                    await this._locker.WaitAsync(token);

                    if (this._runningTaskCancellationTokenSource != null)
                    {
                        linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, this._runningTaskCancellationTokenSource.Token);
                        token = linkedTokenSource.Token;
                    }
                }
                finally
                {
                    this._locker.Release();
                }

                await this._host.WaitForShutdownAsync(token);
            }
            finally
            {
                linkedTokenSource?.Dispose();
            }
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken token = default)
        {
            return StartAsync(null, token);
        }

        /// <summary>
        /// Start the <see cref="ClusterNode"/>, execute <paramref name="runningFunction"/> and give back the hand while server run in background
        /// </summary>
        public async Task StartAsync(Func<IServiceProvider, IMARIExecutionHandler, CancellationToken, Task>? runningFunction, CancellationToken token = default)
        {
            if (this._hostStopped)
                throw new InvalidOperationException("Could not start on application host stop, it could not be restarted plz create a new mari element");

            IReadOnlyCollection<IInitService>? nodeInitServiceAfter = null;
            Task? runningTask = null;
            try
            {
                await this._locker.WaitAsync(token);

                if (this._runningTask == null)
                {
                    this._runningTaskCancellationTokenSource = new CancellationTokenSource();
                    var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, this._runningTaskCancellationTokenSource.Token);
                    this._runningLinkedTokenSource = linkedToken;
                    token = linkedToken.Token;

                    var serviceToInit = this._host.Services.GetServices<IInitService>();

                    var initServices = serviceToInit.Distinct()
                                                    .GroupBy(s => s.ExpectOrleanStarted)
                                                    .ToDictionary(k => k.Key, v => v.ToReadOnly());

                    if (initServices.TryGetValue(true, out var expectStarted))
                        nodeInitServiceAfter = expectStarted;

                    if (initServices.TryGetValue(false, out var beforeStartServices))
                        await InitializeServicesAsync(beforeStartServices, token);

                    await TryStartHostAsync(token);
                }

                runningTask = this._runningTask;
            }
            finally
            {
                this._locker.Release();
            }

            await OnStartAsync(token);

            if (runningTask != null)
            {
                await runningTask;

                if (nodeInitServiceAfter is not null && nodeInitServiceAfter.Any())
                    await InitializeServicesAsync(nodeInitServiceAfter, token);

                if (runningFunction != null)
                    await runningFunction(this._host.Services, this._mariExecutionHandler, token);
            }

            this._isRunning = true;
        }

        /// <summary>
        /// Stop the <see cref="MARIBasePart{TConfig}"/>
        /// </summary>
        public async Task StopAsync(CancellationToken token = default)
        {
            Task? runningTask = null;
            CancellationTokenSource? runningTaskCancellationTokenSource = null;
            CancellationTokenSource? runningLinkedTokenSource = null;
            var locker = this._locker;
            try
            {
                await locker.WaitAsync(token);

                runningTask = this._runningTask;
                if (this._runningTask != null)
                {
                    var serviceToFinalize = this._host.Services.GetServices<IFinalizeService>();

                    var initTasks = serviceToFinalize.Where(s => !s.IsFinalized && !s.IsFinalizing)
                                                     .Select(s => s.FinalizeAsync(token: token))
                                                     .ToArray();

                    try
                    {
                        await initTasks.SafeWhenAllAsync(token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (AggregateException ex)
                    {
                        foreach (var inner in ex.InnerExceptions)
                            this._logger.OptiLog(LogLevel.Critical, "Exceptions on INodeInitService : {exception}", inner);
                    }

                    this._runningTaskCancellationTokenSource?.Cancel();

                    // Detach sources, disposed once the running task is done
                    runningTaskCancellationTokenSource = this._runningTaskCancellationTokenSource;
                    runningLinkedTokenSource = this._runningLinkedTokenSource;

                    this._runningTaskCancellationTokenSource = null;
                    this._runningLinkedTokenSource = null;
                    this._runningTask = null;
                }

                await OnStopAsync(token);
                await TryStopHostAsync(token);
            }
            catch (Exception ex)
            {
                OnStopFailed(ex);
            }
            finally
            {
                locker.Release();
            }

            try
            {
                if (runningTask != null)
                    await runningTask;
            }
            catch (OperationCanceledException)
            {
                // It's normal if the only error is a cancellation : OperationCanceledException
            }
            finally
            {
                // Sources detached in the lock above, dispose them once the running task is done
                runningLinkedTokenSource?.Dispose();
                runningTaskCancellationTokenSource?.Dispose();
            }

            this._isRunning = false;
        }

        /// <summary>
        /// Try start the host if needed
        /// </summary>
        protected virtual Task TryStartHostAsync(CancellationToken token)
        {
            if (this._hostOwned)
                this._runningTask = this._host.StartAsync(token);
            else
                this._runningTask = Task.CompletedTask;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Called clean up and stop the host
        /// </summary>
        protected virtual async Task TryStopHostAsync(CancellationToken token)
        {
            if (this._hostOwned)
            {
                await this._host.StopAsync(token);
                this._hostStopped = true;
            }
        }

        /// <summary>
        /// Called when part stop
        /// </summary>
        [ThreadSafe]
        protected virtual Task OnStopAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when stop action failed.
        /// </summary>
        [ThreadSafe]
        protected virtual void OnStopFailed(Exception ex)
        {
            this._logger.OptiLog(LogLevel.Critical, "MARI node stop failed : {exception}", ex);
        }

        /// <summary>
        /// Called when cluster node start.
        /// </summary>
        protected virtual Task OnStartAsync(CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override async ValueTask DisposeBeginAsync()
        {
            await StopAsync();
        }

        /// <inheritdoc />
        protected override async ValueTask DisposeEndAsync()
        {
            this._locker.Dispose();

            this._runningLinkedTokenSource?.Dispose();
            this._runningTaskCancellationTokenSource?.Dispose();

            // The host (and its root service provider : singletons, sockets, timers, ...) is owned, it MUST be disposed
            if (this._hostOwned)
            {
                if (this._host is IAsyncDisposable asyncDisposableHost)
                    await asyncDisposableHost.DisposeAsync();
                else
                    this._host.Dispose();
            }

            await base.DisposeEndAsync();
        }

        /// <summary>
        /// Configure <see cref="HostBuilder"/>
        /// </summary>
        protected static HostBuilder CreateAndConfigureHost(string[]? args,
                                                            Action<HostBuilderContext, IConfigurationBuilder>? setupConfig)
        {
            var host = new HostBuilder();

            if (args is not null && args.Any())
                host.ConfigureDefaults(args);

            if (setupConfig is not null)
            {
                host.ConfigureAppConfiguration((ctx, builder) =>
                {
                    setupConfig(ctx, builder);
                    builder.ActivateTemplatedConfiguration();
                });
            }

            return host;
        }

        /// <summary>
        /// Initializes the services.
        /// </summary>
        private async Task InitializeServicesAsync(IReadOnlyCollection<IInitService> beforeStartServices, CancellationToken token)
        {
            var initTasks = beforeStartServices.Where(s => !s.IsInitialized && !s.IsInitializing)
                                               .Select(s => s.InitializationAsync(this._host.Services, token: token))
                                               .ToArray();

            try
            {
                await initTasks.SafeWhenAllAsync(token);
            }
            catch (AggregateException ex)
            {
                // Not rethrown to keep the start-up behavior, but each failure is logged as critical with its exception :
                // the related services are NOT initialized
                foreach (var inner in ex.InnerExceptions)
                    this._logger.LogCritical(inner, "IInitService initialization failed, service not initialized : {exception}", inner);

                var notInitialized = beforeStartServices.Where(s => !s.IsInitialized)
                                                        .Select(s => s.GetType().Name)
                                                        .ToArray();

                if (notInitialized.Length > 0)
                    this._logger.LogCritical("IInitService not initialized : {services}", string.Join(", ", notInitialized));
            }
        }

        #endregion
    }
}
