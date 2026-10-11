// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Messenger
{
    using MARI.Toolbox.Abstractions.Supports;

    /// <summary>
    /// A channel is specialized in a type of message and can be extra specialized in a specific category
    /// It store the subscription and distribute the incomming message
    /// </summary>
    internal interface IMessageQueueChannel : ISupportDebugDisplayName
    {
    }
}
