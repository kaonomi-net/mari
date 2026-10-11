// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using MARI.Framework.Core.Abstractions.Sequence;
    using MARI.Framework.Node.Abstractions;

    /// <summary>
    /// Wizard to configure worflows
    /// </summary>
    public interface IMARINodeSequenceWizard
    {
        /// <summary>
        /// Registers a sequence definition.
        /// </summary>
        IMARINodeSequenceWizard Register(params SequenceDefinition[] sequenceDefinition);

        /// <summary>
        /// Registers <typeparamref name="TSequenceModule"/>
        /// </summary>
        IMARINodeSequenceWizard Register<TSequenceModule>() where TSequenceModule : ISequenceModule, new();
    }
}
