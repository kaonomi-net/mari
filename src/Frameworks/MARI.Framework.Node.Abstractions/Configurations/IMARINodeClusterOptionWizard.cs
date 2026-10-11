// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Configurations
{
    using System;

    /// <summary>
    /// Build node - cluster relation options
    /// </summary>
    public interface IMARINodeClusterOptionWizard
    {
        /// <summary>
        /// Define delta between table clean up, removing disconnected nodes
        /// </summary>
        /// <remarks>
        ///     Orlean by default is 7 days
        ///     MARI default is 30 min and if Debugger is attached 5 min
        /// </remarks>
        /// <param name="expirationDelay">Define After how many inactive time a silo is remove from the membership table.</param>
        /// <param name="experiationCheckPeriod">Define the period between each cleanup check, default: 1h</param>
        IMARINodeClusterOptionWizard MembershipTableCleanup(TimeSpan expirationDelay, TimeSpan experiationCheckPeriod);

        /// <summary>
        /// Adds the silo information in the console title
        /// </summary>
        IMARINodeClusterOptionWizard AddConsoleSiloTitleInfo();
    }
}
