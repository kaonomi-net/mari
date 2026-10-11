// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions
{
    using MARI.Toolbox.Abstractions.Supports;

    /// <summary>
    /// Service at node level that need to be finalized at stop
    /// </summary>
    /// <seealso cref="ISupportFinalization" />
    public interface IFinalizeService : ISupportFinalization
    {
    }
}
