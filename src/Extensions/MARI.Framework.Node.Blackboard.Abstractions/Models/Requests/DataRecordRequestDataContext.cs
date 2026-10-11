// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions.Models.Requests
{
    using System;

    /// <summary>
    /// Option cached in the context used to configure a blackdoard's push/pull request behavior automatically
    /// </summary>
    [GenerateSerializer]
    public record struct DataRecordRequestDataContext(Guid BoardId,
                                                      string? LogicalTypePattern,
                                                      Guid? DataRecordUid);
}
