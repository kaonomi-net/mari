// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// Keep : MARI.Framework.Configurations; to easy configuration use
namespace MARI.Framework.Configurations
{
    using MARI.Framework.Node.StreamQueue;
    using MARI.Framework.Node.StreamQueue.Configurations;

    public static class MARINodeWizardStreamQueueExtensions
    {
        /// <summary>
        /// Setup all services to enabled Streaming/Queuing management on this node.
        /// </summary>
        public static IMARINodeWizard UseStreamQueues(this IMARINodeWizard mariNodeWizard, bool configureMARIDefaultStream = true)
        {
            Action<IMARINodeStreamQueueBuilder>? cfg = null;

            if (configureMARIDefaultStream)
                cfg = b => b.SetupDefaultMARIMemoryStream();

            return UseStreamQueues(mariNodeWizard, cfg);
        }

        /// <summary>
        /// Setup all services to enabled Streaming/Queuing management on this node.
        /// </summary>
        public static IMARINodeWizard UseStreamQueues(this IMARINodeWizard mariNodeWizard, Action<IMARINodeStreamQueueBuilder>? cfg)
        {
            mariNodeWizard.ConfigureServices(s => s.AddGrainService<StreamTriggerVGrainService>());

            if (cfg is not null)
            {
                var builder = new MARINodeStreamQueueBuilder(mariNodeWizard.ConfigurationTools);
                cfg(builder);
            }

            return mariNodeWizard;
        }
    }
}