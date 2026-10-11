// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Services
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Sequence;

    /// <summary>
    /// Service used to controler <see cref="IVGrainProvider"/> based on condition
    /// </summary>
    internal interface ISequenceVGrainProvider
    {
        /// <summary>
        /// Gets the grain provider dedicated based on
        /// </summary>
        IVGrainProvider GetGrainProvider(ref readonly SequenceStageDefinition step);
    }
}
