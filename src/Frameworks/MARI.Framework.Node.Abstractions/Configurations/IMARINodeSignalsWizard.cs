// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using MARI.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Setup local signals
    /// </summary>
    public interface IMARINodeSignalsWizard
    {
        /// <summary>
        /// Registers a signals definition.
        /// </summary>
        IMARINodeSignalsWizard Register(params SignalDefinition[] signalDefinition);
    }
}
