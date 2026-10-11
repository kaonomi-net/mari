// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions.Models.Requests
{
    using MARI.Framework.Node.Blackboard.Abstractions.Models.Targets;

    using System;

    /// <summary>
    /// Option used to configure a blackdoard's push/pull request behavior
    /// </summary>
    [GenerateSerializer]
    public record struct DataRecordRequestOption(Guid? BoardId,
                                                 string? BoardName,
                                                 string? BoardTemplateName,
                                                 bool DontExtractDataFromContext = true) : IDataRecordRequestOption;

    /// <summary>
    /// Option used to configure the blackdoard pull behavior
    /// </summary>
    [GenerateSerializer]
    public record struct DataRecordPullRequestOption(Guid? BoardId,
                                                     string? BoardName,
                                                     string? BoardTemplateName,
                                                     int? MaxItems,
                                                     RecordStatusEnum? RecordStatusFilter,
                                                     string? LogicalTypePattern,
                                                     bool DontExtractDataFromContext = true) : IDataRecordPullRequestOption;

    /// <summary>
    /// Option used to configure the blackdoard pull behavior
    /// </summary>
    [GenerateSerializer]
    public record struct DataRecordPushRequestOption(Guid? BoardId,
                                                     string? BoardName,
                                                     string? BoardTemplateName,
                                                     bool ForceValueCreation,
                                                     RecordStatusEnum? NewRecordStatus,
                                                     string? LogicalType,
                                                     DataRecordPushRequestTypeEnum PushActionType,
                                                     bool DontExtractDataFromContext = true) : IDataRecordPushRequestOption;
}
