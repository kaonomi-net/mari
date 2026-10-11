// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Enums
{
    using MARI.Toolbox.Abstractions.Attributes;

    /// <summary>
    /// Enum expositing logical relation
    /// </summary>
    public enum LogicEnum
    {
        None,

        [DescriptionWithAlias('&')]
        And,

        [DescriptionWithAlias('|')]
        Or,

        [DescriptionWithAlias('^')]
        ExclusiveOr,

        [DescriptionWithAlias('!')]
        Not
    }
}
