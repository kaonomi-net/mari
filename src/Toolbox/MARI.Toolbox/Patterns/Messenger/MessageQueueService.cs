// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Messenger
{
    using MARI.Toolbox.Abstractions.Patterns.Messenger;
    using MARI.Toolbox.Abstractions.Proxies;
    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    public sealed class MessageQueueService : SafeDisposable, IMessageQueueService
    {
        #region Fields

        private readonly Dictionary<ChannelKey, IMessageQueueChannel> _channels;
        private readonly ReaderWriterLockSlim _locker;

        private readonly ILogger<IMessageQueueService> _logger;
        private readonly IDispatcherProxy? _dispatcherProxy;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageQueueService"/> class.
        /// </summary>
        public MessageQueueService(ILogger<IMessageQueueService>? logger = null,
                                   IDispatcherProxy? dispatcherProxy = null)
        {
            this._channels = new Dictionary<ChannelKey, IMessageQueueChannel>();
            this._locker = new ReaderWriterLockSlim();

            this._logger = logger ?? NullLogger<IMessageQueueService>.Instance;
            this._dispatcherProxy = dispatcherProxy;
        }

        #endregion

        #region Nested

        /// <summary>
        /// Complex key grouping <paramref name="SubchannelCategory"/> and <paramref name="MessageType"/>
        /// </summary>
        private sealed record class ChannelKey(Type MessageType, string? SubchannelCategory);

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Push<TMessage>(TMessage message, string? category = null)
            where TMessage : IMessage
        {
            Task.Run(async () =>
            {
                try
                {
                    await SendAsync(message, category);
                }
                catch (Exception ex)
                {
                    this._logger.OptiLog(LogLevel.Error, "Error during MessageQueue push message {message} - Category {category}", message, category);
                    this._dispatcherProxy?.Throw(ex, nameof(MessageQueueService) + "." + "Push<" + typeof(TMessage) + ">()");
                }
            }).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async ValueTask SendAsync<TMessage>(TMessage message, string? category = null, CancellationToken token = default)
            where TMessage : IMessage
        {
            var channels = new MessageQueueChannel<TMessage>[2];

            var key = new ChannelKey(typeof(TMessage), category);

            this._locker.EnterReadLock();
            try
            {
                if (this._channels.TryGetValue(key, out var channel))
                    channels[0] = (MessageQueueChannel<TMessage>)channel;

                // A category channel already forwards the message to its parent channel,
                // send directly to the parent only when no category channel exists to avoid double delivery
                if (!string.IsNullOrEmpty(category) && channels[0] is null)
                {
                    var parentKey = new ChannelKey(typeof(TMessage), null);
                    if (this._channels.TryGetValue(parentKey, out var parentChannel))
                        channels[1] = (MessageQueueChannel<TMessage>)parentChannel;
                }
            }
            finally
            {
                this._locker.ExitReadLock();
            }

            for (var i = 0; i < 2; i++)
            {
                var channel = channels[i];
                await (channel?.SendAsync(message, token) ?? ValueTask.CompletedTask);
            }
        }

        /// <inheritdoc />
        public IMessageQueueSubscription<TMessage> Subscribe<TMessage>(string? category = null) where TMessage : IMessage
        {
            this._locker.EnterWriteLock();
            try
            {
                var key = new ChannelKey(typeof(TMessage), category);
                IMessageQueueChannel? channel = null;

                if (!this._channels.TryGetValue(key, out channel))
                {
                    var parentKey = new ChannelKey(typeof(TMessage), null);

                    IMessageQueueChannel? parent = null;

                    if (key != parentKey && !string.IsNullOrEmpty(category))
                    {
                        if (!this._channels.TryGetValue(parentKey, out parent))
                        {
                            parent = new MessageQueueChannel<TMessage>();
                            this._channels.Add(parentKey, parent);
                        }
                    }

                    channel = new MessageQueueChannel<TMessage>((MessageQueueChannel<TMessage>)parent!, category);

                    this._channels.Add(key, channel);
                }

                Debug.Assert(channel is not null);

                return (MessageQueueChannel<TMessage>)channel!;

            }
            finally
            {
                this._locker.ExitWriteLock();
            }
        }

        /// <inheritdoc />
        protected override void DisposeBegin()
        {
            IReadOnlyCollection<IMessageQueueChannel> channels;
            this._locker.EnterWriteLock();
            try
            {
                channels = this._channels.Values.ToArray();
                this._channels.Clear();
            }
            finally
            {
                this._locker.ExitWriteLock();
            }

            foreach (var channel in channels)
            {
                try
                {
                    if (channel is IDisposable disposable)
                        disposable.Dispose();
                }
                catch (ObjectDisposedException)
                { }
                catch (Exception ex)
                {
                    this._logger.OptiLog(LogLevel.Warning, "[Channel: {channel}] dispose failed {exception}", channel.ToDebugDisplayName(), ex);
                }
            }

            base.DisposeBegin();
        }

        /// <inheritdoc />
        protected override void DisposeEnd()
        {
            try
            {
                this._locker?.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
            base.DisposeEnd();
        }

        #endregion
    }
}
