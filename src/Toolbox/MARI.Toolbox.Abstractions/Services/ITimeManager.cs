// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Services
{
    using System;

    /// <summary>
    /// Handler used to provide time information through
    /// </summary>
    public interface ITimeManager
    {
        #region Properties

        /// <summary>
        /// Gets the date of today.
        /// </summary>
        DateOnly Today { get; }

        /// <summary>
        /// Gets the UTC now.
        /// </summary>
        DateTimeOffset Now { get; }

        /// <summary>
        /// Gets the UTC now.
        /// </summary>
        DateTime UtcNow { get; }

        /// <summary>
        /// Gets the UTC now.
        /// </summary>
        DateTime LocalNow { get; }

        #endregion
    }
}
