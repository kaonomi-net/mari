// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Deferred
{
    /// <summary>
    /// 
    /// </summary>
    public enum DeferredStatusEnum
    {
        None,

        /// <summary>
        /// Define the deferred work have been started
        /// </summary>
        Initialize,

        /// <summary>
        /// Define the deferred work is still alive and working
        /// </summary>
        Alive,

        /// <summary>
        /// Define the deferred work have been done
        /// </summary>
        Finished,

        /// <summary>
        /// Define the deferred work have been cancelled
        /// </summary>
        Cancelled,

        /// <summary>
        /// Define a deferred work that failed.
        /// </summary>
        Failed,

        /// <summary>
        /// The cleanup
        /// </summary>
        Cleanup
    }
}
