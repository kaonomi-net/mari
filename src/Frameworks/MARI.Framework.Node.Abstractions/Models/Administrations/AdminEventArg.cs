// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Models.Administrations
{
    using MARI.Framework.Node.Abstractions.Enums;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base class of every admin event information categorise by <paramref name="Type"/> with an etag version
    /// </summary>
    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]    
    public record class AdminEventArg(AdminEventTypeEnum Type, string Etag);
}
