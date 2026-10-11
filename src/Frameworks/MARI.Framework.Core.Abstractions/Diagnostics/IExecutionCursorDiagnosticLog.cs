// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Diagnostics
{
    using System;

    public interface IExecutionCursorDiagnosticLog : IDiagnosticLog
    {
        /// <summary>
        /// Gets the execution cursor.
        /// </summary>
        Guid Cursor { get; }
    }
}
