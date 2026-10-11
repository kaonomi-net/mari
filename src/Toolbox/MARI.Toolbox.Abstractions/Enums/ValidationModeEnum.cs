// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Enums
{
    /// <summary>
    /// Define the type of logical validation between validator of the same group collection
    /// </summary>
    public enum ValidationModeEnum
    {
        None = 0,
        All,
        AtLeastOne,
        NoOne
    }
}
