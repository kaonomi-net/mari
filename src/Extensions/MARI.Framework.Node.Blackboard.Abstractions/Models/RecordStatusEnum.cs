// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions.Models
{
    /// <summary>
    /// Define record status
    /// </summary>
    [Flags]
    public enum RecordStatusEnum
    {
        None = 0,
        Preparation = 1,
        Ready = 2,
        Decommissioned = 4,
    }
}
