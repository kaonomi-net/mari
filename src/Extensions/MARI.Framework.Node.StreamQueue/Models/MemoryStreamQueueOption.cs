// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.StreamQueue.Models
{
    /// <summary>
    /// Option used to create a memory steam queue
    /// </summary>
    public record class MemoryStreamQueueOption(string ConfigContainerName, bool CreateInMemoryPubSubStorage);
}
