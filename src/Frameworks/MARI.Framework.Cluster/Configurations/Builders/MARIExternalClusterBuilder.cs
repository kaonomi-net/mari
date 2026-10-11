// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Cluster.Configurations.Builders
{
    using MARI.Framework.Configurations;

    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Implementation of external information builder
    /// </summary>
    /// <typeparam name="TOption">The type of the option.</typeparam>
    /// <seealso cref="MARIExternalBaseServiceBuilder{TOption, IMARIClusterBuilder{TOption}}" />
    public sealed class MARIExternalClusterBuilder<TOption> : MARIExternalBaseServiceBuilder<TOption, IMARIClusterExternalBuilder<TOption>>, IMARIClusterExternalBuilder<TOption>
        where TOption : class, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MARIExternalClusterBuilder{TOption}"/> class.
        /// </summary>
        public MARIExternalClusterBuilder(IConfiguration configuration)
            : base(configuration)
        {
        }
    }
}
