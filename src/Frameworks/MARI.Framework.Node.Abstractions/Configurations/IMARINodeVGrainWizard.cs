// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using MARI.Framework.Core.Abstractions;

    using System;

    /// <summary>
    /// Wizard used to setup vgrain
    /// </summary>
    public interface IMARINodeVGrainWizard
    {
        /// <summary>
        /// Gets the Wizard root.
        /// </summary>
        IMARINodeWizard Root { get; }

        /// <summary>
        /// Removes all automatic discovery vgrain.
        /// </summary>
        IMARINodeVGrainWizard RemoveAllAutoDiscoveryVGrain();

        /// <summary>
        /// Adds a specific <typeparamref name="TVGrain"/> <see cref="IVGrain"/>
        /// </summary>
        IMARINodeVGrainWizard Add<TVGrain, TVGrainImplementation>()
            where TVGrain : IVGrain
            where TVGrainImplementation : TVGrain;

        /// <summary>
        /// Remoev a specific <typeparamref name="TVGrain"/> <see cref="IVGrain"/>
        /// </summary>
        IMARINodeVGrainWizard Remove<TVGrain>()
            where TVGrain : IVGrain;

        /// <summary>
        /// Remoev a specific <paramref name="vgrain"/> <see cref="IVGrain"/>
        /// </summary>
        IMARINodeVGrainWizard Remove(Type vgrain);

        /// <summary>
        /// Adds a specific <typeparamref name="TVGrain"/> <see cref="IVGrain"/>
        /// </summary>
        IMARINodeVGrainWizard Add(Type vgrainInterface, Type vgrainImplementation);
    }
}
