// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Messenger
{
    using MARI.Toolbox.Abstractions.Patterns.Messenger;
    using MARI.Toolbox.Disposables;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Define a subscription to a channel with specific condition
    /// </summary>
    internal sealed class MessageQueueSubscription<TMessage> : SafeDisposable
        where TMessage : IMessage
    {
        #region Fields

        private readonly Func<TMessage, string?, ValueTask> _callback;

        private readonly Func<TMessage, string?, bool>? _predicate;
        private readonly WeakReference _sourceReference;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageQueueSubscription"/> class.
        /// </summary>
        public MessageQueueSubscription(Guid uid,
                                        WeakReference source,
                                        Func<TMessage, string?, ValueTask> callback,
                                        Func<TMessage, string?, bool>? predicate)
        {
            this.Uid = uid;
            this._predicate = predicate;
            this._callback = callback;
            this._sourceReference = source;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is alive.
        /// </summary>
        public bool IsAlive
        {
            get { return this._sourceReference.IsAlive; }
        }

        /// <summary>
        /// Gets the uid.
        /// </summary>
        public Guid Uid { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Check if the subscription correspond to same parameters
        /// </summary>
        public bool Same<TSource>(in TSource source, Func<TMessage, string?, ValueTask> callback, Func<TMessage, string?, bool>? predicate)
        {
            if (!this._sourceReference.IsAlive)
                return false;

            if (!ReferenceEquals(this._sourceReference.Target, source))
                return false;

            if (this._callback != callback)
                return false;

            if (this._predicate != predicate)
                return false;

            return true;
        }

        /// <inheritdoc cref="IMessageQueueService.SendAsync{TMessage}(TMessage, string?)" />
        public ValueTask SendAsync(TMessage message, string? category)
        {
            if (this.IsAlive == false)
                return ValueTask.CompletedTask;

            if (this._callback is not null)
                return this._callback(message, category);

            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Determines whether this instance can execute the specified message.
        /// </summary>
        internal bool CanExecute(TMessage message, string? subchannelCategory)
        {
            return this._predicate is null || this._predicate(message, subchannelCategory);
        }

        #endregion
    }
}
