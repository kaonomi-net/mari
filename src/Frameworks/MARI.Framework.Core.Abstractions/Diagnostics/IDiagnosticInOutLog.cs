// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Diagnostics
{
    using MARI.Toolbox.Abstractions.Models;

    /// <summary>
    /// Log use store in/out information
    /// </summary>
    /// <seealso cref="IDiagnosticLog" />
    public interface IDiagnosticInOutLog : IDiagnosticLog
    {
        #region Properties

        /// <summary>
        /// Gets the in out data in parameters or out result
        /// </summary>
        TypedArgument? InOut { get; }

        /// <summary>
        /// Gets the error.
        /// </summary>
        string? Error { get; }

        #endregion
    }
}
