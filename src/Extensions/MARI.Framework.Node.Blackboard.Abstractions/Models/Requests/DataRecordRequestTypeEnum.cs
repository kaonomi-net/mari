// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions.Models.Targets
{
    /// <summary>
    /// Define the type of action wish throught <see cref="IDataRecordRequest"/>
    /// </summary>
    public enum DataRecordRequestTypeEnum
    {
        None,
        Push,
        Pull
    }
}
