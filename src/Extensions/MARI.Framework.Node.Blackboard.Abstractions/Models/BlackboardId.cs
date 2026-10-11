// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions.Models
{
    using System;

    /// <summary>
    /// Define a blackboard target information
    /// </summary>
    [GenerateSerializer]
    public record struct BlackboardId(Guid Uid, string BoardName, string BoardTemplateKey);
}
