// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions
{
    using System.Threading.Tasks;

    /// <summary>
    /// Identity and permission of a component
    /// </summary>
    public interface IComponentIdentityCard : IIdentityCard
    {
        /// <summary>
        /// Determines whether this component is enable.
        /// </summary>
        ValueTask<bool> IsEnable();
    }
}
