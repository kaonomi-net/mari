// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Graphs.Map
{
    using System.Collections.Generic;

    /// <summary>
    /// Define option to build the node path
    /// </summary>
    public record struct GetPathOptions(int MaxToleratePathSize = -1,
                                        IReadOnlyCollection<string>? FilterRelationTypes = null,
                                        IReadOnlyCollection<string>? ExcludeRelationTypes = null);
}