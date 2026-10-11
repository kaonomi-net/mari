// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Cluster.Abstractions.Configurations.Builders
{
    using MARI.Framework.Configurations;

    /// <summary>
    /// Define a service allowing custom addin configuration to mari evironement 
    /// </summary>
    /// <seealso cref="IBuilderMARIBaseWizard" />
    /// <seealso cref="IMARIBaseGenericBuilder" />
    public interface IMARIExtensionBuilderTool : IBuilderMARIBaseWizard, IMARIBaseGenericBuilder
    {
        /// <summary>
        /// Try get specific builder
        /// </summary>
        TBuilder TryGetBuilder<TBuilder>();
    }
}
