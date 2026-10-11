// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions
{
    /// <summary>
    /// Placeholder proxy of the root <see cref="IGrainFactory"/> without any redirection rules
    /// </summary>
    /// <seealso cref="Orleans.IGrainFactory" />
    /// <remarks>
    ///     Mainly used by the mari system grain to prevent any by pass
    /// </remarks>
    public interface IGrainOrleanFactory : IGrainFactory
    {
    }
}
