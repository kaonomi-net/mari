// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// Keep : MARI.Framework.Configurations; to easy configuration use
namespace MARI.Framework.Configurations
{
    using MARI.Framework.Node.Signals.Doors;

    public static class MARINodeWizardExtensions
    {
        /// <summary>
        /// Setup all services to enabled signal management on this node.
        /// </summary>
        public static IMARINodeWizard UseSignals(this IMARINodeWizard mariNodeWizard)
        {
            mariNodeWizard.ConfigureServices(s => s.AddGrainService<DoorVGrainService>());
            return mariNodeWizard;
        }
    }
}
