// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.Abstractions.Models
{
    /// <summary>
    /// Result of a remote tool execution
    /// </summary>
    public record class RemoteExecResult(bool Success, string StandardLog, string ErrorLog, int ExitCode, string ExceptionMessage);
}
