// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Patterns.Messenger
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Entry point of messaging services
    /// </summary>
    public interface IMessageQueueService
    {
        /// <summary>
        /// Subscribes to specific message <typeparamref name="TMessage"/>
        /// </summary>
        IMessageQueueSubscription<TMessage> Subscribe<TMessage>(string? category = null) where TMessage : IMessage;

        /// <summary>
        /// Sends a message to all the subscribe items and wait all the subscribers treatment.
        /// </summary>
        ValueTask SendAsync<TMessage>(TMessage message, string? category = null, CancellationToken token = default) where TMessage : IMessage;

        /// <summary>
        /// Sends a message to all the subscribe items. without waiting.
        /// </summary>
        void Push<TMessage>(TMessage message, string? category = null) where TMessage : IMessage;
    }

    /// <summary>
    /// Subscription finalizer used to specify filters and callback for a specific message
    /// </summary>
    public interface IMessageQueueSubscription<TMessage>
        where TMessage : IMessage
    {
        /// <summary>
        /// Subscribes to a specific message with different filters
        /// </summary>
        IDisposable Message<TInstance>(TInstance source, Func<TMessage, string?, ValueTask> callback, Func<TMessage, string?, bool>? predicate = null);
    }
}
