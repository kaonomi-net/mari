// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Test.Interfaces
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Attributes;
    using MARI.Toolbox;

    /// <summary>
    /// Provide Html for testing purpose
    /// </summary>
    /// <seealso cref="IVGrain" />
    [VGrainUsage<NoneType, string>]
    public interface IHtmlProviderTestTransformer : IVGrain
    {
        /// <summary>
        /// Gets a test HTML
        /// </summary>
        Task<string> GetHtmlAsync(IExecutionContext sequenceContext);
    }
}