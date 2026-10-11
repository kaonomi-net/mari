// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions.VGrains
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Attributes;

    /// <summary>
    /// VGrain in charge to build the re
    /// </summary>
    /// <seealso cref="IVGrain" />

    [VGrainStatelessWorker]
    public interface IBlackboardStorageRequestBuilderVGrain : IVGrain
    {
    }
}
