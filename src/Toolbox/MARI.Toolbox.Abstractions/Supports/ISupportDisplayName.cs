// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Supports
{
    /// <summary>
    /// Support a public name that could be directly displayed to user
    /// </summary>
    public interface ISupportDisplayName
    {
        /// <summary>
        /// Gets the name of the display.
        /// </summary>
        string DisplayName { get; }
    }
}
