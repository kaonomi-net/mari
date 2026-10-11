// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions
{
    using MARI.Framework.Core.Abstractions.Models;

    using Orleans.Runtime;

    /// <summary>
    /// Information provider about an <see cref="IVGrain"/>
    /// </summary>
    /// <seealso cref="IGrain" />
    public interface IVGrainInformationProvider
    {
        #region Properties

        /// <summary>
        /// Gets the meta data.
        /// </summary>
        VGrainMetaData MetaData { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the grain identifier.
        /// </summary>
        GrainId GetGrainId();

        #endregion
    }
}
