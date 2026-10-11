// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Configurations.AutoConfigurator
{
    using MARI.Framework.Node.Models;

    using Microsoft.Extensions.Options;

    using Orleans.Configuration;

    using System.Linq;

    internal static class MemoryConfiguratorHelper
    {
        /// <summary>
        /// Options the configurator.
        /// </summary>
        internal static void OptionConfigurator(OptionsBuilder<MemoryGrainStorageOptions> options)
        {
            var defaultMemoryOptionServiceDescriptor = options.Services.Where(s => s.IsKeyedService == false && 
                                                                              s.ImplementationInstance is DefaultMemoryGrainStorageOptions)
                                                                       .LastOrDefault();              

            if (defaultMemoryOptionServiceDescriptor is not null && defaultMemoryOptionServiceDescriptor.ImplementationInstance is DefaultMemoryGrainStorageOptions defaultOption)
            {
                options.PostConfigure(m =>
                {
                    m.NumStorageGrains = (int)defaultOption.NumStorageGrains;
                    m.InitStage = (int)defaultOption.InitStage;
                });
            }
        }
    }
}
