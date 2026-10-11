// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Services
{
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Services;
    using MARI.Framework.Node.Abstractions.Services;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="MARI.Framework.Core.VGrainProvider" />
    /// <seealso cref="MARI.Framework.Node.Abstractions.Services.IVGrainMARISystemProvider" />
    internal sealed class VGrainMARISystemProvider : VGrainProvider, IVGrainMARISystemProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainMARISystemProvider"/> class.
        /// </summary>
        public VGrainMARISystemProvider(IGrainOrleanFactory grainFactory,
                                             IVGrainIdFactory vgrainIdFactory) 
            : base(grainFactory, vgrainIdFactory)
        {
        }

        #endregion
    }
}
