// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Cron
{
    using MARI.Framework.Node.Abstractions.Triggers;

    using Orleans.Runtime;

    using System;

    /// <summary>
    /// Cron Reminder state used to store information and cache to help <see cref="CronTriggerHandlerVGrain"/> to perform
    /// </summary>
    [GenerateSerializer]
    public sealed class CronReminderState : TriggerState
    {
    }
}
