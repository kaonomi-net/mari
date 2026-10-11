// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// KEEP : MARI.Framework.Core.Abstractions
namespace MARI.Framework.Core.Abstractions
{
    using MARI.Framework.Core.Abstractions.Artifacts;

    using System;

    /// <summary>
    /// 
    /// </summary>
    public static class ArtifactExecutableVGrainExtensions
    {
        /// <summary>
        /// Execute external VGrain define by artifact executable
        /// </summary>
        public static IExecutionDirectBuilder<IGenericArtifactExecutableVGrain> ExternalVGrain(this IMARIExecutionHandler handler, Guid artifactId)
        {
            return handler.VGrain<IGenericArtifactExecutableVGrain>(artifactId);
        }
    }
}
