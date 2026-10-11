// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using MARI.Framework.Cluster.Abstractions.Configurations.Builders;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Artifacts;
    using MARI.Framework.Core.Abstractions.Doors;
    using MARI.Framework.Core.Abstractions.Sequence;
    using MARI.Framework.Core.Abstractions.Signals;
    using MARI.Framework.Core.Abstractions.Streams;
    using MARI.Framework.Core.Abstractions.Triggers;
    using MARI.Framework.Node.Abstractions.Configurations;

    using System;

    /// <summary>
    /// Builder used to setup in node local memory all the elements trigger, cron, signals, artefacts, ...
    /// </summary>
    /// <remarks>
    ///     Attention the element define each are mainly define in the local node. <br />
    ///     Except if the other node declare the same definitions they will not be able on cluster
    /// </remarks>
    public interface IMARINodeLocalDefinitionsBuilder
    {
        #region Properties

        /// <summary>
        /// Gets the gneric builder tool.
        /// </summary>
        public IMARIExtensionBuilderTool ConfigurationTools { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Setup callable definitions
        /// </summary>
        IMARINodeLocalDefinitionsBuilder Setup<TDefinition>(params TDefinition[] definitions)
            where TDefinition : IDefinition, IRefDefinition;

        /// <summary>
        /// Setup callable sequence
        /// </summary>
        IMARINodeLocalDefinitionsBuilder SetupSequences(Action<IMARINodeSequenceWizard> config);

        /// <summary>
        /// Setup callable sequence
        /// </summary>
        IMARINodeLocalDefinitionsBuilder SetupSequences(params SequenceDefinition[] sequences);

        /// <summary>
        /// Setups the triggers.
        /// </summary>
        IMARINodeLocalDefinitionsBuilder SetupTriggers(Action<IMARINodeTriggersWizard> config);

        /// <summary>
        /// Setups the triggers.
        /// </summary>
        IMARINodeLocalDefinitionsBuilder SetupTriggers(params TriggerDefinition[] triggers);

        /// <summary>
        /// Setups the signals.
        /// </summary>
        IMARINodeLocalDefinitionsBuilder SetupSignals(Action<IMARINodeSignalsWizard> config);

        /// <summary>
        /// Setups the signals.
        /// </summary>
        IMARINodeLocalDefinitionsBuilder SetupSignals(params SignalDefinition[] signals);

        /// <summary>
        /// Setups the doors.
        /// </summary>
        IMARINodeLocalDefinitionsBuilder SetupDoors(Action<IMARINodeDoorsWizard> config);

        /// <summary>
        /// Setups the doors.
        /// </summary>
        IMARINodeLocalDefinitionsBuilder SetupDoors(params DoorDefinition[] doors);

        /// <summary>
        /// Setups the artifact resource, external source code (python, c++, exe, dll, png, ...)
        /// </summary>
        IMARINodeLocalDefinitionsBuilder SetupArtifacts(Action<IMARINodeArtifactsWizard> artifactResourceBuilder);

        /// <summary>
        /// Setups artifacts resources, external source code (python, c++, exe, dll, png, ...)
        /// </summary>
        IMARINodeLocalDefinitionsBuilder SetupArtifacts(params ArtifactDefinition[] artifactResourceDefinitions);

        /// <summary>
        /// Setups the stream queues.
        /// </summary>
        IMARINodeLocalDefinitionsBuilder SetupStreamQueues(Action<IMARINodeStreamQueueWizard> streamQueueResourceBuilder);

        /// <summary>
        /// Setups the stream queues.
        /// </summary>
        IMARINodeLocalDefinitionsBuilder SetupStreamQueues(params StreamQueueDefinition[] streamQueueDefinitions);

        #endregion
    }
}
