// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Extensions
{
    using MARI.Framework.Core.Abstractions.Diagnostics;
    using MARI.Toolbox.Extensions;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class DiagnosticLogExtensions
    {
        #region Methods

        /// <summary>
        /// Converts to debug string display
        /// </summary>
        public static string ToDebugDisplayName(this IEnumerable<IDiagnosticLog> logs)
        {
            var parentIndx = logs.GroupBy(l => l.CallerId ?? Guid.Empty)
                                 .ToDictionary(k => k.Key, k => k.OrderBy(o => o.Orientation)
                                                                 .ThenBy(o => o.CreateOn)
                                                                 .ToReadOnly());

            return ToDebugDisplayName(parentIndx, Guid.Empty, new HashSet<Guid>());
        }

        /// <summary>
        /// Converts to debug string display
        /// </summary>
        /// <param name="visited">Parent ids already displayed, used to prevent infinite recursion on cyclic ids</param>
        private static string ToDebugDisplayName(IReadOnlyDictionary<Guid, IReadOnlyCollection<IDiagnosticLog>> parentIndx, Guid parentId, HashSet<Guid> visited)
        {
            var builder = new StringBuilder();

            if (visited.Add(parentId) && parentIndx.TryGetValue(parentId, out var logs))
            {
                foreach (var grp in logs.GroupBy(l => l.CurrentExecutionId))
                {
                    foreach (var log in grp)
                    {
                        builder.Append("* ");
                        builder.AppendLine(log.ToDebugDisplayName());
                    }

                    if (grp.Key == parentId || visited.Contains(grp.Key))
                        continue;

                    var childStr = ToDebugDisplayName(parentIndx, grp.Key, visited).Replace("\n", "\n    ");
                    if (!string.IsNullOrEmpty(childStr))
                    {
                        builder.Append(' ', 4);
                        builder.AppendLine(childStr);
                    }
                }
            }

            return builder.ToString();
        }

        #endregion
    }
}
