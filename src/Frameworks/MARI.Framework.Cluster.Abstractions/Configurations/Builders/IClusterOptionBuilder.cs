// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    /// <summary>
    /// Build used to setup generic cluster options
    /// </summary>
    public interface IClusterOptionBuilder
    {
        /// <summary>
        /// Blocks the automatic configuration.
        /// </summary>
        IClusterOptionBuilder BlockAutoConfig();
    }
}
