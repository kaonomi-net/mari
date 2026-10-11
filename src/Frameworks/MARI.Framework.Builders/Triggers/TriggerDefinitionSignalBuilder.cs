// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.Implementations.Triggers
{
    using MARI.Framework.Builders.Triggers;
    using MARI.Framework.Core.Abstractions.Doors;
    using MARI.Framework.Core.Abstractions.Signals;
    using MARI.Framework.Core.Abstractions.Triggers;

    /// <summary>
    /// Trigger base on signal listenig
    /// </summary>
    /// <seealso cref="TriggerDefinitionBaseBuilder" />
    /// <seealso cref="ITriggerDefinitionFinalizeBuilder" />
    internal sealed class TriggerDefinitionSignalBuilder : TriggerDefinitionWithInputBaseBuilder, ITriggerDefinitionFinalizeBuilder, ITriggerDefinitionSignalBuilder, ITriggerDefinitionDoorBuilder
    {
        #region Fields

        private readonly DoorId? _doorId;
        private readonly SignalId? _signalId;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TriggerDefinitionSignalBuilder"/> class.
        /// </summary>
        public TriggerDefinitionSignalBuilder(TriggerTypeEnum triggerType,
                                              SignalId? signalId,
                                              DoorId? doorId,
                                              string simpleNameIdentifier,
                                              string displayName,
                                              Guid? fixUid = null,
                                              Action<IDefinitionMetaDataBuilder>? metadataBuilder = null)
            : base(triggerType, simpleNameIdentifier, displayName, fixUid, metadataBuilder)
        {
            this._signalId = signalId;
            this._doorId = doorId;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override TriggerDefinition Build()
        {
            if (this._signalId == null && this._doorId == null)
                throw new NullReferenceException("At least a Signal or Door must be register");

            return new SignalTriggerDefinition(this.Uid,
                                               GetRefId(),
                                               this.DisplayName,
                                               this.Targets,
                                               true,
                                               this._signalId,
                                               this._doorId,
                                               this.DefinitionMetaData,
                                               this.TriggerGlobalOutputDefinition);
        }

        #endregion
    }
}
