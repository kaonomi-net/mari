// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Services
{
    using MARI.Framework.Core.Abstractions.Customizations;

    using MARI.Toolbox.Abstractions.Disposables;

    /// <summary>
    /// Service used to provide a dedicated <see cref="ISequenceVGrainProvider"/>
    /// </summary>
    internal interface ISequenceVGrainProviderFactory
    {
        /// <summary>
        /// Gets the grain provider dedicated based on
        /// </summary>
        ISafeDisposable<ISequenceVGrainProvider> GetProvider(in ExecutionCustomizationDescriptions? customization);
    }
}
