// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Disposables
{
    using System;

    /// <summary>
    /// Container used to carry multiple disposable token and dispose them when dispose itself
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface ISafeDisposableContainer : IDisposable
    {
        /// <summary>
        /// Pushe the token in the container.
        /// </summary>
        void PushToken<TToken>(TToken token) where TToken : IDisposable;
    }
}
