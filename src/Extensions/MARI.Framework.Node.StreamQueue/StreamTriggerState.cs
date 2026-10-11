// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.StreamQueue
{
    using MARI.Framework.Node.Abstractions.Triggers;

    using Orleans.Streams;

    using System.Runtime.Serialization;

    /// <summary>
    /// State used to stored trigger informations
    /// </summary>
    /// <seealso cref="MARI.Framework.Node.Abstractions.Triggers.TriggerState" />
    [GenerateSerializer]
    [DataContract]
    public class StreamTriggerState : TriggerState
    {
        [IgnoreDataMember]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public StreamSubscriptionHandle<object>? StreamSubscription { get; set; }
    }
}
