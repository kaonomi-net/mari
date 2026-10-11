// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Artifacts
{
    using MARI.Framework.Cluster.Abstractions.Services;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Artifacts;
    using MARI.Framework.Core.Abstractions.Exceptions;
    using MARI.Framework.Node.Abstractions.Artifacts;

    using MARI.Toolbox.Abstractions.Services;
    using MARI.Toolbox.Communications;
    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Extensions;
    using MARI.Toolbox.Services;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json.Linq;

    using System;
    using System.Diagnostics;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Executor used to handled and controlled a remote virtual grain in one shot.
    /// Argument are pass by command line
    /// </summary>
    /// <seealso cref="SafeAsyncDisposable" />
    /// <seealso cref="IArtifactExternalCodeExecutor" />
    public class ExternalCodeDeamonExecutor : ExternalCodeBaseExecutor, IArtifactExternalCodeExecutor
    {
        #region Fields

        private readonly CancellationTokenSource _executorLifeTimeSource;
        private readonly IProcessSystemService _processSystemService;
        private readonly INetworkInspector _networkInspector;
        private readonly DelayTimer _stopTimer;
        private readonly SemaphoreSlim _locker;

        private ComClientProxy? _remoteClient;
        private ComServer? _comServer;
        private IDisposable[]? _processOutputSubscriptions;
        private int _inflightAsks;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalCodeDeamonExecutor"/> class.
        /// </summary>
        public ExternalCodeDeamonExecutor(ArtifactExecutableDefinition artifactExecutableDefinition,
                                          IProcessSystemService processSystemService,
                                          IJsonSerializer jsonSerializer,
                                          INetworkInspector networkInspector,
                                          IConfiguration configuration,
                                          Uri? workingDirectory)
            : base(artifactExecutableDefinition, jsonSerializer, configuration, workingDirectory)
        {
            this._processSystemService = processSystemService;
            this._networkInspector = networkInspector;

            this._executorLifeTimeSource = new CancellationTokenSource();
            this._stopTimer = DelayTimer.Create(OnStopTimerElapsedAsync, startDelay: TimeSpan.FromMinutes(5));
            this._locker = new SemaphoreSlim(1);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the processor.
        /// </summary>
        public IExternalProcess? Processor { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Ensure process is start
        /// </summary>
        public async override ValueTask StartAsync(IExecutionContext executionContext, ILogger logger, CancellationToken cancellationToken)
        {
            CheckAndThrowIfDisposed();

            using (var launchingCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            using (await this._locker.LockAsync(launchingCancelTokenSource.Token))
            {
                if (this.Processor != null)
                {
                    // If so try to ping
                    var proxies = this._comServer?.GetClientProxies();
                    if (proxies != null && proxies.Count == 1)
                    {
                        if (await proxies.First().PingAsync(launchingCancelTokenSource.Token))
                            return;
                    }

                    // if Ping failed KillExisting
                    await ThreadSafeKillTargetRemoteAsync(cancellationToken);
                }

                ExtractCommandLineExec(out var executor, out var args);

                IExternalProcess? processor = null;

                try
                {
                    using (var portReserved = this._networkInspector.GetAndReservedNextUnusedPort(30000, 65536))
                    {
                        JoinArgument(args, "--port", portReserved.Token.ToString());

                        Debug.Assert(this._comServer is null, "Comserver is still alive on remote");
                        this._comServer = new ComServer(portReserved.Token);

                        var clientTask = this._comServer.WaitNextClientAsync(launchingCancelTokenSource.Token);
                        await this._comServer.StartAsync(launchingCancelTokenSource.Token);

                        if (this.ArtifactExecutableDefinition.Verbose == ArtifactExecVerboseEnum.Full)
                            logger.OptiLog(LogLevel.Information, "Launch External VGrain {app} {args}", this.ArtifactExecutableDefinition.DisplayName, args);

                        processor = await LaunchProcess(executor, args, this.ArtifactExecutableDefinition, launchingCancelTokenSource.Token);

                        // Track the process immediately to be able to kill it if the startup failed
                        this.Processor = processor;

                        // If process failed then it MUST stop the client connection
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        processor.GetAwaiterTask().ContinueWith(_ =>
                        {
                            try
                            {
                                launchingCancelTokenSource.Cancel();
                            }
                            catch (ObjectDisposedException)
                            {
                                // Process exited after the launch phase, nothing to cancel anymore
                            }
                        }).ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods

                        // Subscriptions are bound to the process life time, released in ThreadSafeKillTargetRemoteAsync
                        var standardToken = processor.StandardOutputObservable.Subscribe(s => logger.OptiLog(LogLevel.Information, "{log}", s));
                        var errorToken = processor.ErrorOutputObservable.Subscribe(s => logger.OptiLog(LogLevel.Error, "{log}", s));
                        this._processOutputSubscriptions = new IDisposable[] { standardToken, errorToken };

                        this._remoteClient = await clientTask;

                        if (await this._remoteClient.PingAsync(cancellationToken) == false)
                            throw new ArtifactRemoteException(this.ArtifactExecutableDefinition.Uid, "Ping failed", executionContext);
                    }
                }
                catch (Exception ex)
                {
                    await ThreadSafeKillTargetRemoteAsync(default);

                    if (processor is not null && processor.ErrorOutput.Any())
                    {
                        ex = new ArtifactExecutionException(this.ArtifactExecutableDefinition.Uid,
                                                            processor.ErrorOutput.AggregateStrings(),
                                                            executionContext,
                                                            ex);
                    }

                    logger.OptiLog(LogLevel.Critical, "External execution error {exception}, Failed to launch or ping issues", ex);
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Start timer to stop the deamon
        /// </summary>
        public override async ValueTask StopAsync(IExecutionContext executionContext, ILogger logger, CancellationToken cancellationToken)
        {
            CheckAndThrowIfDisposed();
            await this._stopTimer.StartAsync();
        }

        /// <inheritdoc />
        public sealed override async ValueTask<TOutput?> AskAsync<TOutput, TInput>(TInput? input,
                                                                                   IExecutionContext executionContext,
                                                                                   ILogger logger,
                                                                                   CancellationToken cancellationToken)
            where TOutput : default
            where TInput : default
        {
            CheckAndThrowIfDisposed();

            using (var localAskCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this._executorLifeTimeSource.Token, cancellationToken))
            {
                await this._stopTimer.StopAsync();

                // Snapshot the client under the lock and flag the ask as in-flight to prevent the stop timer from killing the process mid-request
                ComClientProxy? remoteClient;
                using (await this._locker.LockAsync(localAskCancelTokenSource.Token))
                {
                    remoteClient = this._remoteClient;
                    if (remoteClient is null)
                        throw new ArtifactRemoteException(this.ArtifactExecutableDefinition.Uid, "Remote process is not started", executionContext);

                    Interlocked.Increment(ref this._inflightAsks);
                }

                try
                {
                    using (var logToken = remoteClient.Subscribe(msg => ManagedSystemMessage(msg, executionContext, logger)))
                    {
                        var cmdBase64 = FormatCommand(input, executionContext);

                        var responseBase64 = await remoteClient.AskAsync(Encoding.UTF8.GetBytes(cmdBase64), localAskCancelTokenSource.Token);

                        if (responseBase64 == null)
                            return default;

                        localAskCancelTokenSource.Token.ThrowIfCancellationRequested();

                        return ManagedClientResult<TOutput>(Encoding.UTF8.GetString(responseBase64),
                                                            executionContext,
                                                            null,
                                                            null,
                                                            logger);
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref this._inflightAsks);
                }
            }
        }

        #region Tools

        /// <summary>
        /// Launches the process.
        /// </summary>
        protected virtual async Task<IExternalProcess> LaunchProcess(string executor, List<string> args, ArtifactExecutableDefinition definition, CancellationToken token)
        {
            return await this._processSystemService.StartAsync(executor,
                                                               this.WorkingDir!.LocalPath,
                                                               token,
                                                               args.ToArray());
        }

        /// <summary>
        /// Manageds the system message.
        /// </summary>
        private void ManagedSystemMessage(ComClientProxy.UnmanagedMessage message, IExecutionContext executionContext, ILogger logger)
        {
            if (message == null || message.Message == null || message.Message.Length == 0 || message.Type != ComClientProxy.MessageType.System)
                return;

            var messageStr = Encoding.UTF8.GetString(message.Message);
            var jobj = JObject.Parse(messageStr);

            if (!jobj.TryGetValue("ExecutionId", StringComparison.OrdinalIgnoreCase, out var token) ||
                token == null ||
                token is JValue prop == false ||
                prop.Value == null ||
                prop.Type != JTokenType.String ||
                !Guid.TryParse(prop.Value<string>(), out var id) ||
                id != executionContext.CurrentExecutionId)
            {
                return;
            }

            var type = jobj.GetValue("Type");
            var msg = jobj.GetValue("Message");

            var logLevel = LogLevel.Information;
            if (jobj.TryGetValue("Level", StringComparison.OrdinalIgnoreCase, out var levelToken) &&
                levelToken is JValue lvlValue &&
                (lvlValue.Type == JTokenType.Integer || lvlValue.Type == JTokenType.String))
            {
                if (lvlValue.Type == JTokenType.String && Enum.TryParse<LogLevel>(lvlValue.Value<string>(), out var result))
                    logLevel = result;
                else if (lvlValue.Type == JTokenType.Integer && lvlValue.Value<int>() <= (int)LogLevel.None && lvlValue.Value<int>() >= (int)LogLevel.Trace)
                    logLevel = (LogLevel)lvlValue.Value<int>();
            }

            string? msgStr = string.Empty;

            if (msg is JValue msgValue)
            {
                msgStr = msgValue.Value<string>();
                if (!string.IsNullOrEmpty(msgStr))
                    msgStr = Encoding.UTF8.GetString(Convert.FromBase64String(msgStr));
            }

            base.ManagedExecutionMessage((type as JValue)?.Value<string>(),
                                         logLevel,
                                         msgStr,
                                         logger);

        }

        /// <summary>
        /// Called when the stop delay elapsed; kill the remote process only if no request is in progress.
        /// </summary>
        /// <remarks>
        /// Each request calls <see cref="StopAsync"/> at the end, which restarts the timer.
        /// </remarks>
        private async Task OnStopTimerElapsedAsync(CancellationToken token)
        {
            using (await this._locker.LockAsync(token))
            {
                if (Volatile.Read(ref this._inflightAsks) > 0)
                    return;

                await ThreadSafeKillTargetRemoteAsync(token);
            }
        }

        /// <summary>
        /// Kills the target remote asynchronous.
        /// </summary>
        private async Task KillTargetRemoteAsync(CancellationToken token)
        {
            using (await this._locker.LockAsync(token))
            {
                await ThreadSafeKillTargetRemoteAsync(token);
            }
        }

        /// <summary>
        /// Kills the target remote asynchronous.
        /// </summary>
        private async Task ThreadSafeKillTargetRemoteAsync(CancellationToken token)
        {
            this._remoteClient = null;

            var subscriptions = this._processOutputSubscriptions;
            this._processOutputSubscriptions = null;

            if (subscriptions is not null)
            {
                foreach (var subscription in subscriptions)
                    subscription.Dispose();
            }

            var tmp = this._comServer;
            this._comServer = null;

            if (tmp is not null)
                await tmp.DisposeAsync();

            var processorTmp = this.Processor;
            this.Processor = null;

            if (processorTmp != null)
                await processorTmp.KillAsync(token);
        }

        /// <inheritdoc />
        protected override async ValueTask DisposeBeginAsync()
        {
            this._executorLifeTimeSource.Cancel();

            await this._stopTimer.StopAsync();
            await KillTargetRemoteAsync(default);
        }

        /// <inheritdoc />
        protected override async ValueTask DisposeEndAsync()
        {
            await this._stopTimer.DisposeAsync();
            this._executorLifeTimeSource.Dispose();
            this._locker.Dispose();
        }

        #endregion

        #endregion
    }
}
