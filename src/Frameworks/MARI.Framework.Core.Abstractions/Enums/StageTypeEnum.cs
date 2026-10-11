// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Enums
{
    /// <summary>
    /// Stage type
    /// </summary>
    public enum StageTypeEnum
    {
        None,
        Call,
        Foreach,
        Filter,
        Convert,
        PushToContext,
        FireSignal,
        Select,
        NestedSequenceCall
    }
}
