// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions
{
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Factory in charge to generate default vgrain id based on description attribute used
    /// </summary>
    public interface IVGrainIdDefaultFactory
    {
        #region Methods

        /// <summary>
        /// Builds a new vgrain identifier base on information provide by the <paramref name="input"/> and <paramref name="executionContext"/>
        /// </summary>
        IVGrainId BuildNewId(Type vgrainType, object? input, IExecutionContext? executionContext, ILogger logger);

        #endregion
    }
}
