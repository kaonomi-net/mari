// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Supports
{
    /// <summary>
    /// Support converting this instance into debuggable string
    /// </summary>
    public interface ISupportDebugDisplayName
    {
        /// <summary>
        /// Convert instance to string with debug information
        /// </summary>
        string ToDebugDisplayName();
    }
}
