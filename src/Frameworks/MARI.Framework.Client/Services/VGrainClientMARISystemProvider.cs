// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Client.Services
{
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Services;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class VGrainClientMARISystemProvider : VGrainProvider, IVGrainMARISystemProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainClientMARISystemProvider"/> class.
        /// </summary>
        public VGrainClientMARISystemProvider(IVGrainIdFactory grainIdFactory, IGrainFactory grainFactory)
            : base(grainFactory, grainIdFactory)
        {
            
        }

        #endregion
    }
}
