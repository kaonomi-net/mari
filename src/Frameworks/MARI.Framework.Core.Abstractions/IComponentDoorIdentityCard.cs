// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions
{
    using System.Threading.Tasks;

    /// <summary>
    /// Component information dedicate to door
    /// </summary>
    /// <seealso cref="IComponentIdentityCard" />
    public interface IComponentDoorIdentityCard : IComponentIdentityCard
    {
        /// <summary>
        /// Determines whether the door could be stimulate or should only accumulate inputs
        /// </summary>
        ValueTask<bool> CanBeStimuate();
    }
}
