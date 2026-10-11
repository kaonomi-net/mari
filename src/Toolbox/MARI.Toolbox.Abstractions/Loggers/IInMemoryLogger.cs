// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Loggers
{
    using Microsoft.Extensions.Logging;

    using System.Collections.Generic;

    /// <summary>
    ///  Logger that keep in memory the logs
    /// </summary>
    public interface IInMemoryLogger : ILogger, IObservable<SimpleLog>
    {
        #region Methods

        /// <summary>
        /// Gets a copy of logs at requested time.
        /// </summary>
        IReadOnlyCollection<SimpleLog> GetLogsCopy();

        #endregion
    }
}
