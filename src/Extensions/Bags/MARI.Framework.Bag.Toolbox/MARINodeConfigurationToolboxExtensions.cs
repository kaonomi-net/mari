// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using MARI.Framework.Bag.Toolbox.Abstractions;
    using MARI.Framework.Bag.Toolbox.Grains;

    /// <summary>
    /// Configure the toolbox vgrain, services ...
    /// </summary>
    public static class MARINodeConfigurationToolboxExtensions
    {
        /// <summary>
        /// Adds the debug tools, (Sequence, vgrins ...).
        /// </summary>
        public static IMARINodeWizard AddToolBoxTools(this IMARINodeWizard mariNodeWizard)
        {
#pragma warning disable IDE0053 // Use expression body for lambda expression
            mariNodeWizard.SetupNodeVGrains(s =>
            {
                s.Add<IDelayVGrain, DelayVGrain>();
            });
#pragma warning restore IDE0053 // Use expression body for lambda expression

            return mariNodeWizard;
        }
    }
}