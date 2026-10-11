// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.Abstractions.Services
{
    using MARI.Toolbox.WPF.Abstractions.Models;

    /// <summary>
    /// Handler installation and remote communication
    /// </summary>
    public interface IRemoteExecHandler
    {
        /// <summary>
        /// Executes  target external after installed
        /// </summary>
        Task<RemoteExecResult> ExecuteAsync(CancellationToken token, params string[] arguments);
    }
}
