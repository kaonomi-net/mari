// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Sequence.Stages
{
    public interface IFlowHostStageDefinition
    {
        /// <summary>
        /// Gets the inner flow
        /// </summary>
        SequenceDefinition InnerFlow { get; }
    }
}
