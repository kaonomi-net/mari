// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.Abstractions.Services
{
    using System;

    /// <summary>
    /// Factory used to create <see cref="IRemoteExecHandler"/>
    /// </summary>
    public interface IRemoteExecHandlerFactory
    {
        /// <summary>
        /// Gets the remote execute handler from an external provider
        /// </summary>
        IRemoteExecHandler GetRemoteExecHandler(Uri uri, string toolName, string executableName);
    }
}
