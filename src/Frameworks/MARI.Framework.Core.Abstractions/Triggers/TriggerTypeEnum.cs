// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Triggers
{
    /// <summary>
    /// Define the type of trigger
    /// </summary>
    public enum TriggerTypeEnum
    {
        None,
        Manual,
        Cron,
        Signal,
        Other,
        Stream
    }
}
