// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Communications
{
    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Helpers;

    using System.Diagnostics;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using static MARI.Toolbox.Communications.ComClientProxy;

    /// <summary>
    /// Proxy to send or received data from a client
    /// </summary>
    public sealed class ComClientProxy : SafeDisposable, IObservable<UnmanagedMessage>
    {
        #region Fields

        private static readonly int s_guidSize;

        private readonly Dictionary<Guid, TaskCompletionSource<byte[]>> _pendingResultTasks;
        private readonly ComClient _comClient;

        private readonly IDisposable _observerToken;
        private readonly Subject<UnmanagedMessage> _subject;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ComClientProxy"/> class.
        /// </summary>
        static ComClientProxy()
        {
            var emptyBytes = Guid.Empty.ToString();
            s_guidSize = emptyBytes.Length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComClientProxy"/> class.
        /// </summary>
        internal ComClientProxy(ComClient comClient)
        {
            this._pendingResultTasks = new Dictionary<Guid, TaskCompletionSource<byte[]>>();

            this._comClient = comClient;

            this._comClient.ClientLeaveEvent -= ComClient_ClientLeaveEvent;
            this._comClient.ClientLeaveEvent += ComClient_ClientLeaveEvent;

            this.Uid = Guid.NewGuid();

            var observer = Observer.Create<byte[]>(OnMessageReceived, OnComplete);
            this._observerToken = this._comClient.Subscribe(observer);

            this._subject = new Subject<UnmanagedMessage>();
            var connectable = this._subject.SubscribeOn(TaskPoolScheduler.Default)
                                           .Publish();

            var connectObservableToken = connectable.Connect();
            RegisterDisposableDependency(connectObservableToken);
        }

        #endregion

        #region Nested

        /// <summary>
        /// Define the type of message carry
        /// </summary>
        public enum MessageType : byte
        {
            User = 0,
            System = 1,
            Ping = 2,
            Pong = 3,
            Error = byte.MaxValue,
        }

        public record UnmanagedMessage(MessageType Type, string MessageId, byte[] Message);

        #endregion

        #region Properties

        /// <summary>
        /// Gets the client uid.
        /// </summary>
        public Guid Uid { get; }

        /// <summary>
        /// Gets the endpoint. (local or remote)
        /// </summary>
        public EndPoint? Endpoint
        {
            get { return this._comClient.Endpoint; }
        }

        #endregion

        #region Events

        /// <summary>
        /// The client leave event
        /// </summary>
        public event Action<ComClientProxy>? ClientLeavedEvent;

        #endregion

        #region Method

        /// <summary>
        /// Ping target to ensure client is repondings
        /// </summary>
        public async Task<bool> PingAsync(CancellationToken cancellationToken)
        {
            try
            {
                using (var timeoutCancelToken = CancellationHelper.DisposableTimeout(TimeSpan.FromSeconds(5)))
                using (var token = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancelToken.Content))
                {
                    var sendUid = SendImpl(MessageType.Ping, null, out var waitingResultTask, true, token.Token, null);
                    await waitingResultTask!.Task.WaitAsync(token.Token);
                }
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (WebException)
            {
                // Connection lost
                return false;
            }
            return true;
        }

        /// <summary>
        /// Asks data
        /// </summary>
        public async Task<byte[]> AskAsync(byte[] data, CancellationToken cancellationToken, Guid? forceMessageId = null)
        {
            using (var timeoutCancelToken = CancellationHelper.DisposableTimeout(TimeSpan.FromSeconds(Debugger.IsAttached ? 50000 : 5)))
            using (var token = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancelToken.Content))
            {

                var sendUid = SendImpl(MessageType.User, data, out var waitingResultTask, true, token.Token, forceMessageId);

                var results = await waitingResultTask!.Task.WaitAsync(token.Token);
                if (results != null && results.Any())
                    return results;

                return EnumerableHelper<byte>.ReadOnlyArray;
            }
        }

        /// <summary>
        /// Sends data without expecting answer
        /// </summary>
        public ValueTask<Guid> SendAsync(byte[] data, CancellationToken cancellationToken, Guid? forceMessageId = null)
        {
            using (var timeoutCancelToken = CancellationHelper.DisposableTimeout(TimeSpan.FromSeconds(Debugger.IsAttached ? 50000 : 5)))
            using (var token = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancelToken.Content))
            {
                var sendUid = SendImpl(MessageType.User, data, out var _, false, token.Token, forceMessageId);
                return ValueTask.FromResult(sendUid);
            }
        }

        /// <summary>
        /// Relay error
        /// </summary>
        public ValueTask<Guid> RelayErrorAsync(Exception ex, string? extraInfo, CancellationToken cancellationToken, Guid sourceMessageId)
        {
            using (var timeoutCancelToken = CancellationHelper.DisposableTimeout(TimeSpan.FromSeconds(Debugger.IsAttached ? 50000 : 5)))
            using (var token = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancelToken.Content))
            {
                var fullStringError = "Remote source " + sourceMessageId + Environment.NewLine + "Message: " + ex.GetFullString();

                if (!string.IsNullOrEmpty(extraInfo))
                    fullStringError = extraInfo + Environment.NewLine + fullStringError;

                var errorData = Encoding.UTF8.GetBytes(fullStringError);

                var sendUid = SendImpl(MessageType.Error, errorData, out var _, false, token.Token, sourceMessageId);
                return ValueTask.FromResult(sendUid);
            }
        }

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<UnmanagedMessage> observer)
        {
            return this._subject.Subscribe(observer);
        }

        #region Tools

        /// <inheritdoc />
        private void ComClient_ClientLeaveEvent(ComClient obj)
        {
            IReadOnlyCollection<TaskCompletionSource<byte[]>> pendings;

            lock (this._pendingResultTasks)
            {
                pendings = this._pendingResultTasks.Values.ToArray();
                this._pendingResultTasks.Clear();
            }

            var exception = new WebException("Connection losted Uid " + this.Uid + " Endpoint " + obj.Endpoint);

            foreach (var task in pendings)
                task.TrySetException(exception);

            this.ClientLeavedEvent?.Invoke(this);
        }

        /// <inheritdoc />
        private void OnMessageReceived(byte[] msg)
        {
            // Malformed frame : at least the message type and the message id are expected
            if (msg == null || msg.Length < 1 + s_guidSize)
                return;

            ReadOnlySpan<byte> msgSpan = msg;

            var type = (MessageType)msgSpan[0];

            var msgIdStr = Encoding.UTF8.GetString(msgSpan.Slice(1, s_guidSize));

            var msgRemains = msgSpan.Slice(1 + s_guidSize).ToArray();

            if (Guid.TryParse(msgIdStr, out var uid))
            {
                TaskCompletionSource<byte[]>? resultTask;

                lock (this._pendingResultTasks)
                {
                    this._pendingResultTasks.Remove(uid, out resultTask);
                }

                if (resultTask != null)
                {
                    if (type == MessageType.Error)
                        resultTask.TrySetException(new Exception(Encoding.UTF8.GetString(msgRemains)));
                    else
                        resultTask.TrySetResult(msgRemains);
                    return;
                }
            }

            if (type == MessageType.Ping)
            {
                // Echo the ping message id to allow the sender to match the pong
                SendImpl(MessageType.Pong, null, out _, false, default, Guid.TryParse(msgIdStr, out var pingId) ? pingId : null);
                return;
            }

            this._subject.OnNext(new UnmanagedMessage(type, msgIdStr, msgRemains));
        }

        /// <inheritdoc />
        private void OnComplete()
        {
            Dispose();
        }

        /// <summary>
        /// Sends message
        /// </summary>
        private Guid SendImpl(MessageType type,
                              byte[]? messages,
                              out TaskCompletionSource<byte[]>? task,
                              bool createWaitTask,
                              CancellationToken cancellationToken,
                              Guid? forceMessageId)
        {
            var msg = FormatData(type, messages, out var msgId, forceMessageId);
            task = null;

            if (createWaitTask)
            {
                lock (this._pendingResultTasks)
                {
                    task = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
                    this._pendingResultTasks.Add(msgId, task);
                }

                var pendingTask = task;
                cancellationToken.Register(() =>
                {
                    RemovePendingTask(msgId, pendingTask);
                    pendingTask.TrySetCanceled();
                });
            }

            try
            {
                this._comClient.Send(msg);
            }
            catch
            {
                if (task is not null)
                    RemovePendingTask(msgId, task);
                throw;
            }

            return msgId;
        }

        /// <summary>
        /// Removes the pending task registered for <paramref name="msgId"/> if it is still <paramref name="task"/>
        /// </summary>
        private void RemovePendingTask(Guid msgId, TaskCompletionSource<byte[]> task)
        {
            lock (this._pendingResultTasks)
            {
                if (this._pendingResultTasks.TryGetValue(msgId, out var current) && ReferenceEquals(current, task))
                    this._pendingResultTasks.Remove(msgId);
            }
        }

        /// <summary>
        /// Formats the data.
        /// </summary>
        private byte[] FormatData(MessageType ping, byte[]? messages, out Guid messageId, Guid? forceMessageId)
        {
            messageId = forceMessageId ?? Guid.NewGuid();
            var idArray = Encoding.UTF8.GetBytes(messageId.ToString());

            var data = new byte[(messages?.Length ?? 0) + 1 + s_guidSize];

            data[0] = (byte)ping;
            idArray.CopyTo(data, 1);
            messages?.CopyTo(data, 1 + idArray.Length);

            return data;
        }

        /// <summary>
        /// Start to disposed
        /// </summary>
        protected override void DisposeBegin()
        {
            this._observerToken.Dispose();

            IReadOnlyCollection<TaskCompletionSource<byte[]>> tasks;

            lock (this._pendingResultTasks)
            {
                tasks = this._pendingResultTasks.Values.ToArray();
                this._pendingResultTasks.Clear();
            }

            foreach (var task in tasks)
                task.TrySetCanceled();

            base.DisposeBegin();
        }

        #endregion

        #endregion
    }
}
