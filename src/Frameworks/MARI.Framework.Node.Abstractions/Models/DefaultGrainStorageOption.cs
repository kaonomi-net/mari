// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Models
{
    /// <summary>
    /// Option about in memory custom storage
    /// </summary>
    public record class DefaultGrainStorageOption(string StorageKey, bool BuildReadRepository);
}
