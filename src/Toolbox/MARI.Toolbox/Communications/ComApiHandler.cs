// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Communications
{
    using MARI.Toolbox.Abstractions.Services;
    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Services;
    using MARI.Toolbox.Tasks;

    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    using static MARI.Toolbox.Communications.ComClientProxy;

    /// <summary>
    /// Handled communication through <see cref="ComServer"/> && <see cref="ComClient"/>
    /// </summary>
    public sealed class ComApiHandler : SafeAsyncDisposable
    {
        #region Fields

        private static readonly MethodInfo s_processUnmanagedMessageWithResponseMthd;
        private static readonly MethodInfo s_responseWaitingTaskHandling;

        private readonly object _locker = new object();

        private const string RESPONSE_ROUTE_SUFFIX = "/response";

        private readonly IReadOnlyDictionary<string, ComApiDescriptor.ApiHandler> _apiHandlers;
        private readonly IReadOnlyDictionary<string, ComApiDescriptor.ApiHandler> _responseRoutes;
        private readonly ComHandlerObserver _observer;
        private readonly string _identification;

        private readonly ISerializer _serializer;

        private readonly Dictionary<Guid, ITaskCompletionSourceEx> _waitingRequestResult;

        private static readonly short s_rteSizeValueSizeof;
        private ComClientProxy _clientProxy;

        private IDisposable? _subscribeToken;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ComApiHandler"/> class.
        /// </summary>
        static ComApiHandler()
        {
            Expression<Func<ComApiHandler, UnmanagedMessage, ValueTask<int>, ValueTask>> processUnmanagedMessageWithResponse = (c, m, hdl) => c.ProcessUnmanagedMessageWithResponseAsync<int>(m, string.Empty, hdl);
            s_processUnmanagedMessageWithResponseMthd = ((MethodCallExpression)processUnmanagedMessageWithResponse.Body).Method.GetGenericMethodDefinition();

            Expression<Action<ComApiHandler, int, Guid?>> responseWaitingTaskHandling = (c, m, hdl) => c.ResponseWaitingTaskHandling<int>(m, hdl!.Value);
            s_responseWaitingTaskHandling = ((MethodCallExpression)responseWaitingTaskHandling.Body).Method.GetGenericMethodDefinition();

            s_rteSizeValueSizeof = sizeof(ushort);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComApiHandler"/> class.
        /// </summary>
        private ComApiHandler(ComClientProxy clientProxy,
                              ComApiDescriptor apiDescriptor,
                              string identification,
                              ISerializer? serializer)
        {
            this._identification = identification;
            this._serializer = serializer ?? SystemJsonSerializer.Instance;

            this._observer = new ComHandlerObserver(this);
            this._clientProxy = clientProxy;

            this._clientProxy.ClientLeavedEvent -= ClientProxy_ClientLeavedEvent;
            this._clientProxy.ClientLeavedEvent += ClientProxy_ClientLeavedEvent;

            this._waitingRequestResult = new Dictionary<Guid, ITaskCompletionSourceEx>();

            this._apiHandlers = apiDescriptor.Handlers
                                             .ToDictionary(k => k.Route, StringComparer.OrdinalIgnoreCase);

            this._responseRoutes = this._apiHandlers.Values
                                                    .Where(v => v.ExpectResult is not null)
                                                    .ToDictionary(k => k.Route + RESPONSE_ROUTE_SUFFIX, kv => kv);

            this._subscribeToken = this._clientProxy.Subscribe(this._observer);
        }

        #endregion

        #region Event

        /// <summary>
        /// The client leave event
        /// </summary>
        public event Action<ComApiHandler>? ClientLeavedEvent;

        #endregion

        #region Nested

        /// <summary>
        /// 
        /// </summary>
        private sealed class ComHandlerObserver : IObserver<UnmanagedMessage>
        {
            #region Fields

            private readonly ComApiHandler _comApiHandler;

            #endregion

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="ComHandlerObserver"/> class.
            /// </summary>
            /// <param name="comApiHandler">The COM API handler.</param>
            public ComHandlerObserver(ComApiHandler comApiHandler)
            {
                this._comApiHandler = comApiHandler;
            }

            #endregion

            /// <inheritdoc />
            public void OnCompleted()
            {
            }

            /// <inheritdoc />
            public void OnError(Exception error)
            {
                this._comApiHandler.FailAllWaitingRequests(error);
            }

            /// <inheritdoc />
            public void OnNext(UnmanagedMessage value)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await this._comApiHandler.ProcessUnmanagedMessageAsync(value);
                    }
                    catch (Exception ex)
                    {
                        await this._comApiHandler.ProcessUnmanagedMessageErrorAsync(value, ex);
                    }
                }).ConfigureAwait(false);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates communication handler.
        /// </summary>
        public static ComApiHandler Create(ComClientProxy clientProxy, ComApiDescriptor apiDescriptor, string identification, ISerializer? serializer = null)
        {
            return new ComApiHandler(clientProxy, apiDescriptor, identification, serializer);
        }

        /// <summary>
        /// Create a <see cref="ComApiHandler"/> connecting to server
        /// </summary>
        public static async Task<ComApiHandler> ConnectAsync(IPAddress address,
                                                             int port,
                                                             ComApiDescriptor apiDescriptor,
                                                             string identification,
                                                             CancellationToken token = default,
                                                             ISerializer? serializer = null)
        {
            var tcpClient = new TcpClient();
            try
            {
                var client = new ComClient(tcpClient, default);
                await tcpClient.ConnectAsync(address, port, token);
                var proxy = new ComClientProxy(client);
                client.Start();
                return new ComApiHandler(proxy, apiDescriptor, identification, serializer);
            }
            catch
            {
                tcpClient.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Sends the command.
        /// </summary>
        public async ValueTask SendCommand<TMessage>(TMessage message, string? route = null, CancellationToken token = default, Guid? forceMessageId = null)
        {
            await SendImpl(message, route, token, forceMessageId);
        }

        /// <summary>
        /// Sends the command.
        /// </summary>
        public async ValueTask<TResult?> SendRequest<TMessage, TResult>(TMessage message, string? route = null, CancellationToken token = default, Guid? forceMessageId = null)
        {
            forceMessageId ??= Guid.NewGuid();

            TaskCompletionSourceEx<TResult?>? waitingResult = null;

            lock (this._locker)
            {
                if (this._waitingRequestResult.TryGetValue(forceMessageId.Value, out var existing))
                {
                    waitingResult = (TaskCompletionSourceEx<TResult?>)existing;
                }
                else
                {
                    waitingResult = new TaskCompletionSourceEx<TResult?>(null);
                    this._waitingRequestResult.Add(forceMessageId.Value, waitingResult);
                }
            }

            try
            {
                await SendImpl(message, route, token, forceMessageId);

                return await waitingResult.Task.WaitAsync(token);
            }
            finally
            {
                lock (this._locker)
                {
                    // Remove the entry only if it is still the one this call is waiting on
                    if (this._waitingRequestResult.TryGetValue(forceMessageId.Value, out var current) && ReferenceEquals(current, waitingResult))
                        this._waitingRequestResult.Remove(forceMessageId.Value);
                }
            }
        }

        #region Tools

        /// <summary>
        /// Sends the command.
        /// </summary>
        private async ValueTask SendImpl<TMessage>(TMessage message, string? route, CancellationToken token, Guid? forceMessageId)
        {
            var rte = ComApiDescriptor.ExtractRoute(route, typeof(TMessage));

            var msgSerializedBytes = this._serializer.Serialize(message); //JsonConvert.SerializeObject(message, s_jsonSettings);

            var rteBytes = Encoding.UTF8.GetBytes(rte);
            //var msgSerializedBytes = Encoding.UTF8.GetBytes(msgSerialized);

            var data = new byte[s_rteSizeValueSizeof + rteBytes.Length + msgSerializedBytes.Length];

            var rteSizeBytes = BitConverter.GetBytes((ushort)rteBytes.Length);

            rteSizeBytes.CopyTo(data, 0);
            rteBytes.CopyTo(data, s_rteSizeValueSizeof);
            msgSerializedBytes.CopyTo(data, s_rteSizeValueSizeof + rteBytes.Length);

#if DEBUG
            var exampleCopy = rteSizeBytes.Concat(rteBytes)
                                          .Concat(msgSerializedBytes)
                                          .ToArray();

            Debug.Assert(data.SequenceEqual(exampleCopy));
#endif

            await this._clientProxy.SendAsync(data, token, forceMessageId: forceMessageId);
        }

        /// <summary>
        /// Processes the unmanaged message error asynchronous.
        /// </summary>
        private async ValueTask ProcessUnmanagedMessageErrorAsync(UnmanagedMessage value, Exception ex)
        {
            await this._clientProxy.RelayErrorAsync(ex, this._identification, default, Guid.Parse(value.MessageId));
        }

        /// <summary>
        /// Processes the unmanaged message asynchronous.
        /// </summary>
        private ValueTask ProcessUnmanagedMessageAsync(UnmanagedMessage value)
        {
            if (value.Type == MessageType.Error)
            {
                ITaskCompletionSourceEx? sourceTask = null;

                lock (this._locker)
                {
                    if (Guid.TryParse(value.MessageId, out var msgId) &&
                        this._waitingRequestResult.TryGetValue(msgId, out var waitingResponses))
                    {
                        this._waitingRequestResult.Remove(msgId);
                        sourceTask = waitingResponses;
                    }
                }

                sourceTask?.TrySetException(new Exception(Encoding.UTF8.GetString(value.Message)));
                return ValueTask.CompletedTask;
            }

            ReadOnlySpan<byte> messageSpan = value.Message;
            var rteSize = BitConverter.ToUInt16(messageSpan.Slice(0, s_rteSizeValueSizeof));
            var rte = Encoding.UTF8.GetString(messageSpan.Slice(s_rteSizeValueSizeof, rteSize));
            
            var msgSerialized = messageSpan.Slice(s_rteSizeValueSizeof + rteSize);

            if (this._apiHandlers.TryGetValue(rte, out var handler) && handler.Callback is not null)
            {
                var msg = this._serializer.Deserialize(msgSerialized, handler.MessageType);
                var result = handler.Callback.DynamicInvoke(msg, this._clientProxy.Uid);

                ValueTask? resultTask = null;

                if (handler.ExpectResult is null && result is ValueTask castResultTask)
                {
                    resultTask = castResultTask;
                }
                else if (handler.ExpectResult is not null)
                {
                    resultTask = (ValueTask)s_processUnmanagedMessageWithResponseMthd.MakeGenericMethodWithCache(handler.ExpectResult)
                                                                                     .Invoke(this, new object[] { value, rte, result! })!;
                }
                else
                {

                }

                if (resultTask is not null)
                    return resultTask.Value;
            }
            else if (this._responseRoutes.TryGetValue(rte, out var responseHandler) && responseHandler.ExpectResult is not null)
            {
                var response = this._serializer.Deserialize(msgSerialized, responseHandler.ExpectResult);
                //JsonConvert.DeserializeObject(msgSerialized, responseHandler.ExpectResult, s_jsonSettings);

                s_responseWaitingTaskHandling.MakeGenericMethodWithCache(responseHandler.ExpectResult)
                                             .Invoke(this, new object?[] { response, Guid.Parse(value.MessageId) });

                return ValueTask.CompletedTask;
            }

            throw new InvalidOperationException("Invalid API message processing, or route not founded route: " + rte);
        }

        private async ValueTask ProcessUnmanagedMessageWithResponseAsync<TResponse>(UnmanagedMessage valueSource, string route, ValueTask<TResponse> execTask)
        {
            var messageIdInfo = Guid.Parse(valueSource.MessageId);

            var response = await execTask;

            await SendImpl(response, route + RESPONSE_ROUTE_SUFFIX, default, messageIdInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        private void ResponseWaitingTaskHandling<TResponse>(TResponse? response, Guid messageIdInfo)
        {
            ITaskCompletionSourceEx? waitingResponseTask = null;

            lock (this._locker)
            {
                if (this._waitingRequestResult.TryGetValue(messageIdInfo, out var waitingResponses))
                {
                    this._waitingRequestResult.Remove(messageIdInfo);
                    waitingResponseTask = waitingResponses;
                }
            }

            waitingResponseTask?.TrySetResultObject(response);
        }

        /// <summary>
        /// Clients the proxy client leaved event.
        /// </summary>
        private void ClientProxy_ClientLeavedEvent(ComClientProxy obj)
        {
            var exception = new WebException("Connection losted Uid " + this._clientProxy.Uid + " Endpoint " + obj.Endpoint);

            FailAllWaitingRequests(exception);

            this.ClientLeavedEvent?.Invoke(this);
        }

        /// <summary>
        /// Fails all the requests waiting for a response with <paramref name="exception"/>
        /// </summary>
        private void FailAllWaitingRequests(Exception exception)
        {
            ITaskCompletionSourceEx[] taskCompletionSourceExes;
            lock (this._locker)
            {
                taskCompletionSourceExes = this._waitingRequestResult.Values.ToArray();
                this._waitingRequestResult.Clear();
            }

            foreach (var task in taskCompletionSourceExes)
                task.TrySetException(exception);
        }

        /// <inheritdoc />
        protected override ValueTask DisposeBeginAsync()
        {
            this._subscribeToken?.Dispose();
            this._subscribeToken = null;

            this._clientProxy?.Dispose();
            this._clientProxy = null!;

            return base.DisposeBeginAsync();
        }

        #endregion

        #endregion
    }
}
