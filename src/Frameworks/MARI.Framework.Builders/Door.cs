// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders
{
    using MARI.Framework.Builders.Doors;

    using System;

    /// <summary>
    /// Builder used to create <see cref="SignalDefinition"/>. Signal is a complex Signal
    /// </summary>
    public static class Door
    {
        /// <summary>
        /// Creates <see cref="DoorDefinitions"/>
        /// </summary>
        public static IDoorBuilder Create(string simpleNameIdentifier, string? displayName = null, Guid? fixUid = null, Action<IDefinitionMetaDataBuilder>? metaDataBuilder = null)
        {
            return new DoorStartBuilder(simpleNameIdentifier, displayName ?? simpleNameIdentifier, fixUid, metaDataBuilder);
        }
    }
}
