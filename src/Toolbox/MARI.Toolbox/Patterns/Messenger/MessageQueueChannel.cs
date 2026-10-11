// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Messenger
{
    using MARI.Toolbox.Abstractions.Patterns.Messenger;
    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Memories;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Channel in message queue used to subscribe and managed a specific message
    /// </summary>
    internal sealed class MessageQueueChannel<TMessage> : SafeDisposable, IMessageQueueChannel, IMessageQueueSubscription<TMessage>
        where TMessage : IMessage
    {
        #region Fields

        private readonly SafeContainer<MessageQueueSubscription<TMessage>> _messageQueueSubscriptions;

        private readonly MessageQueueChannel<TMessage>? _parent;
        private readonly string? _subchannelCategory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageQueueChannel{TMessage}"/> class.
        /// </summary>
        public MessageQueueChannel(MessageQueueChannel<TMessage>? parent = null, string? subchannelCategory = null)
        {
            this._messageQueueSubscriptions = new SafeContainer<MessageQueueSubscription<TMessage>>();
            this._subchannelCategory = subchannelCategory;
            this._parent = parent;
        }

        #endregion

        #region Nested

        #endregion

        #region Methods

        /// <inheritdoc />
        public IDisposable Message<TInstance>(TInstance source, Func<TMessage, string?, ValueTask> callback, Func<TMessage, string?, bool>? predicate = null)
        {
            var subscriptionId = this._messageQueueSubscriptions.GetExistingId(m => m.Same(source, callback, predicate));

            if (subscriptionId == null)
            {
                subscriptionId = this._messageQueueSubscriptions.Register(new MessageQueueSubscription<TMessage>(Guid.NewGuid(),
                                                                                                                 new WeakReference(source),
                                                                                                                 callback,
                                                                                                                 predicate));
            }

            return new DisposableAction<Guid>(Unsubscribe, subscriptionId.Value);
        }

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        public async ValueTask SendAsync(TMessage message, CancellationToken token = default)
        {
            var callSubscriptions = this._messageQueueSubscriptions.GetContainerCopy(f => f.IsAlive && f.CanExecute(message, this._subchannelCategory));

            var processingTasks = callSubscriptions.Select(c => c.SendAsync(message, this._subchannelCategory))
                                                   .ToArray();

            await processingTasks.SafeWhenAllAsync(token);

            if (this._parent is not null)
                await this._parent.SendAsync(message, token);
        }

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            return "[Message: {0}] [Categeory: {1}]".WithArguments(typeof(TMessage).Name, this._subchannelCategory);
        }

        #region Tools

        /// <summary>
        /// Unsubscribes the specified unique identifier.
        /// </summary>
        private void Unsubscribe(Guid subscriptionId)
        {
            this._messageQueueSubscriptions.Remove(subscriptionId);
        }

        #endregion
     
        #endregion
    }
}
