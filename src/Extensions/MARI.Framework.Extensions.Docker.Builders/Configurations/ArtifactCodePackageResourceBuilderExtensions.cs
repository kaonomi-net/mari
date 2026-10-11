// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// KEEP : MARI.Framework.Builders
namespace MARI.Framework.Builders
{
    /// <summary>
    /// 
    /// </summary>
    public static class ArtifactCodePackageResourceBuilderExtensions
    {
        /// <summary>
        /// Installs the mari python library
        /// </summary>
        public static IArtifactCodePackageResourceDockerEnvironmentBuilder InstallMARIPython(this IArtifactCodePackageResourceDockerEnvironmentBuilder builder)
        {
            builder.AddExtraBuildInstruction("RUN pip install mari");
            return builder;
        }
    }
}
