// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Models
{
    using Orleans.Configuration;

    public record class DefaultMemoryGrainStorageOptions(uint NumStorageGrains = 10, uint InitStage = MemoryGrainStorageOptions.DEFAULT_INIT_STAGE);
}
