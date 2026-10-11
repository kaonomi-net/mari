// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Builders
{
    /// <summary>
    /// Builder used to setup Globalization resources
    /// </summary>
    public interface IGlobalizationStringResourceBuilder
    {
        /// <summary>
        /// Add RESXs the string resource.
        /// </summary>
        IGlobalizationStringResourceBuilder RESXStringResource<TResource>();
    }
}
