// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders
{
    using MARI.Framework.Builders.Artifacts;

    /// <summary>
    /// Artifact builder
    /// </summary>
    public static class Artifact
    {
        /// <summary>
        /// Get a VGrain code artifact 
        /// </summary>
        public static IArtifactExecutablePackageResourceBuilder VGrain(string simpleNameIdentifier,
                                                                       string? displayName = null,
                                                                       Guid? uid = null)
        {
            return new ArtifactExecutablePackageBuilder(simpleNameIdentifier, displayName, uid);
        }
    }
}
