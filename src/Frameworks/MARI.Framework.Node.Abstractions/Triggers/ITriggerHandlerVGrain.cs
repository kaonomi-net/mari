// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Triggers
{
    using MARI.Framework.Core.Abstractions;

    /// <summary>
    /// Handler vgrain associate to a trigger kind
    /// </summary>
    public interface ITriggerHandlerVGrain : IVGrain
    {
        /// <summary>
        /// Update trigger status based on <see cref="ITriggerDefinition"/>
        /// </summary>
        Task UpdateAsync(GrainCancellationToken token);
    }
}
