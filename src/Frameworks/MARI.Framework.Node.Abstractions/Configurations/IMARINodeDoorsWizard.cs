// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using MARI.Framework.Core.Abstractions.Doors;

    /// <summary>
    /// Setup local doors
    /// </summary>
    public interface IMARINodeDoorsWizard
    {
        /// <summary>
        /// Registers a signals definition.
        /// </summary>
        IMARINodeDoorsWizard Register(params DoorDefinition[] signalDefinition);
    }
}
