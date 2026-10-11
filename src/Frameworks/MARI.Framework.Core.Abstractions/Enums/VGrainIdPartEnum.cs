// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Enums
{
    using System;

    /// <summary>
    /// Part that compose a VGrain Id
    /// </summary>
    [Flags]
    public enum VGrainIdPartEnum
    {
        None,
        GrainType = 1,
        Primary = 2,
        Extension = 4,
        All = VGrainIdPartEnum.GrainType | VGrainIdPartEnum.Primary | VGrainIdPartEnum.Extension
    }
}
