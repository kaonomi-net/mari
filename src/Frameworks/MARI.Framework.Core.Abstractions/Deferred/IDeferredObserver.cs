// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Deferred
{
    using System.Threading.Tasks;

    /// <summary>
    /// Observer used to be notify on Deferred work changed
    /// </summary>
    /// <seealso cref="IGrainObserver" />
    public interface IDeferredObserver : IGrainObserver
    {
        /// <summary>
        /// Receive a notification to inform a deferred work change
        /// </summary>
        Task DeferredStatusChangedNotification(DeferredStatusMessage statusChangeMessage);
    }
}
