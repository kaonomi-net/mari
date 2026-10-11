// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Signals
{
    using MARI.Framework.Node.Signals.Models;

    using System.Collections.Generic;

    /// <summary>
    /// Signal state handler
    /// </summary>
    internal sealed class SignalHandlerState : SignalSubscriptionState, IEquatable<SignalHandlerState>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalHandlerState"/> class.
        /// </summary>
        public SignalHandlerState(IEnumerable<SignalSubscription> subscriptions)
            : base(subscriptions)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool Equals(SignalHandlerState? other)
        {
            return base.Equals((SignalSubscriptionState?)other);
        }

        #endregion
    }
}
