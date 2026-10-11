// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Models.SignalMessage
{
    using MARI.Framework.Core.Abstractions.Enums;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// 
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct CollectionChangeSignalMessage<TEntityId>(CollectionChangeTypeEnum ChangeType, IReadOnlyCollection<TEntityId> ImpactedIds);
}
