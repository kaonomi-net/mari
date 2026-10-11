// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// Keep : MARI.Framework.Node.Configurations
namespace MARI.Framework.Configurations
{
    using MARI.Framework.Bag.DebugTools;
    using MARI.Framework.Bag.DebugTools.Grains;
    using MARI.Framework.Bag.DebugTools.Models;
    using MARI.Framework.Builders;
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions.Doors;
    using MARI.Framework.Core.Abstractions.Signals;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Extensions methods used to configure all the debug tools
    /// </summary>
    public static class MARINodeConfigurationDebugToolExtensions
    {
        /// <summary>
        /// Adds the debug tools, (Sequence, vgrins ...).
        /// </summary>
        public static IMARINodeWizard AddDebugTools(this IMARINodeWizard mariNodeWizard, LogLevel logLevel)
        {
            return mariNodeWizard.AddDebugTools(new DebugDisplayInfoOptions(logLevel));
        }

        /// <summary>
        /// Adds the debug tools, (Sequence, vgrins ...).
        /// </summary>
        public static IMARINodeWizard AddDebugTools(this IMARINodeWizard mariNodeWizard, DebugDisplayInfoOptions? option = null)
        {
            if (option != null)
                mariNodeWizard.AddNodeOption(option);

            mariNodeWizard.SetupNodeVGrains(g =>
            {
                g.Add<IDisplayInfoVGrain, DisplayAllInfoVGrain>()
                 .Add<IDisplaySignalsInfoVGrain, DisplaySignalsInfoVGrain>();
            });

            var signalSebugSec = Sequence.Build("display-signal",
                                                nameof(MARI.Framework.Bag) + ":" + nameof(MARI.Framework.Bag.DebugTools) + ":" + nameof(DebugToolConstants.DisplaySignalSequence), 
                                                DebugToolConstants.DisplaySignalSequence, 
                                                o =>
                                                {
                                                    o.PreventSequenceExecutorStateStorage()
                                                     .MinimalLogLevel(option?.LogLevel ?? DebugDisplayInfoOptions.Default.LogLevel);
                                                },
                                                m =>
                                                {
                                                    m.Namespace(DebugToolConstants.BAG_NAMESPACE)
                                                     .AddTags("debug", "sequence", "display")
                                                     .CategoryChain("utilities", "debug");
                                                })
                                                .RequiredInput<SignalMessage>()
                                                .Use<IDisplaySignalsInfoVGrain>().Call((g, s, ctx) => g.DisplaySignalInfoAsync(s, ctx)).Return
                                                .Build();

            mariNodeWizard.AddInMemoryDefinitionProvider(s => s.SetupSequences(signalSebugSec));

            return mariNodeWizard;
        }

        /// <summary>
        /// Get triggers that will display on the logger the signal each time this one is fire
        /// </summary>
        public static IMARINodeWizard ShowSignals(this IMARINodeWizard mariNodeWizard, params SignalDefinition[] signalDefinitions)
        {
            mariNodeWizard.AddInMemoryDefinitionProvider(s =>
            {
                var triggers = signalDefinitions.Select(d => Trigger.Signal(d,
                                                                            "auto-signal-trigger-display-" + RefIdHelper.GetSimpleNameIdentification(d.RefId),
                                                                            "AutoTriggerShow:" + d.DisplayName,
                                                                            d.Uid,
                                                                            metadataBuilder : m => m.Namespace(DebugToolConstants.BAG_NAMESPACE))
                                                                    .AddTargetSequence(DebugToolConstants.DisplaySignalSequence)
                                                                    .Build())
                                                .ToArray();

                s.SetupTriggers(triggers);
            });

            return mariNodeWizard;
        }

        /// <summary>
        /// Get triggers that will display on the logger the door each time this one fire
        /// </summary>
        public static IMARINodeWizard ShowDoors(this IMARINodeWizard mariNodeWizard, params DoorDefinition[] doorDefinitions)
        {
            mariNodeWizard.AddInMemoryDefinitionProvider(s =>
            {
                var triggers = doorDefinitions.Select(d => Trigger.Door(d,
                                                                        "auto-door-trigger-display-" + RefIdHelper.GetSimpleNameIdentification(d.RefId),
                                                                        "AutoTriggerShow:" + d.DisplayName,
                                                                        d.Uid,
                                                                        metadataBuilder: m => m.Namespace(DebugToolConstants.BAG_NAMESPACE))
                                                                  .AddTargetSequence(DebugToolConstants.DisplaySignalSequence)
                                                                  .Build())
                                              .ToArray();

                s.SetupTriggers(triggers);
            });

            return mariNodeWizard;
        }
    }
}
