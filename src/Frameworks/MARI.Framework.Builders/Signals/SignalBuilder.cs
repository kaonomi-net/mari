// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.Signals
{
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions.Enums;
    using MARI.Framework.Core.Abstractions.Signals;

    using System;

    /// <inheritdoc cref="ISignalBuilder" />
    internal sealed class SignalBuilder : SignalNetworkBasePartBuilder<ISignalBuilder>, ISignalBuilder
    {
        #region Fields
        
        private SignalDefinition? _parent;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalBuilder"/> class.
        /// </summary>
        public SignalBuilder(string simpleNameIdentifier, string? displayName = null, Guid? uid = null, Action<IDefinitionMetaDataBuilder>? metaDataBuilder = null)
            : base(simpleNameIdentifier, displayName, uid, metaDataBuilder)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ISignalBuilder Parent(SignalDefinition parent)
        {
            this._parent = parent;
            return this;
        }

        /// <inheritdoc />
        public SignalDefinition Build()
        {
            return new SignalDefinition(this.Uid,
                                        RefIdHelper.Generate(RefTypeEnum.Signal, this.SimpleNameIdentifier, this.DefinitionMetaData?.NamespaceIdentifier),
                                        this.DisplayName ?? this.SimpleNameIdentifier,
                                        this.DefinitionMetaData,
                                        this._parent?.SignalId);
        }

        #endregion
    }
}
