// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using MARI.Framework.Node.Abstractions.Models;

    /// <summary>
    /// Specific configuration related to node only
    /// </summary>
    /// <seealso cref="IMARICoreConfigurationWizard" />
    public interface IMARINodeConfigurationWizard : IMARICoreConfigurationWizard<IMARINodeConfigurationWizard>
    {
        /// <summary>
        /// Adds the diagnostic options.
        /// </summary>
        IMARINodeConfigurationWizard AddDiagnostic(ClusterNodeDiagnosticOptions options);

        /// <summary>
        /// Enables the diagnostic log to be relay to classic logger.
        /// </summary>
        IMARINodeConfigurationWizard EnableDiagnosticRelayToLogger();
    }
}
