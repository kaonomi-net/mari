// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Deferred
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Define a id structure used to access deferred reponse
    /// </summary>
    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct DeferredId(Guid Uid, Guid SourceId);
}
