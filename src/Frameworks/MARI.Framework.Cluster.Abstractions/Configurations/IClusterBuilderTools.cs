// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using MARI.Framework.Cluster.Abstractions.Services;
    using MARI.Toolbox.Abstractions.Services;

    /// <summary>
    /// Contains all tools needed during the mari cluster part setup
    /// </summary>
    public interface IClusterBuilderTools
    {
        /// <summary>
        /// Gets the assembly inspector services.
        /// </summary>
        IAssemblyInspector AssemblyInspector { get; }

        /// <summary>
        /// Gets the assembly loader.
        /// </summary>
        IAssemblyLoader AssemblyLoader { get; }

        /// <summary>
        /// Gets the file system handler.
        /// </summary>
        IFileSystemHandler FileSystemHandler { get; }

        /// <summary>
        /// Gets the network inspector.
        /// </summary>
        INetworkInspector NetworkInspector { get; }
    }
}
