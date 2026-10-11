// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.UnitTests.ToolKit.Helpers
{
    using Microsoft.Extensions.Logging;

    using Orleans;
    using Orleans.Runtime;

    using System;

    public sealed class TestGrainLifecycle : LifecycleSubject, IGrainLifecycle
    {
        public TestGrainLifecycle(ILogger logger)
            : base(logger)
        {
        }

        public void AddMigrationParticipant(IGrainMigrationParticipant participant)
        {
            throw new NotImplementedException();
        }

        public void RemoveMigrationParticipant(IGrainMigrationParticipant participant)
        {
            throw new NotImplementedException();
        }
    }
}
