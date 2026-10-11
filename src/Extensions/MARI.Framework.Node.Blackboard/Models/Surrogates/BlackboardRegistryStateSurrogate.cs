// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)
namespace MARI.Framework.Node.Blackboard.Models.Surrogates
{
    using MARI.Framework.Node.Blackboard.Abstractions.Models;
    using MARI.Framework.Node.Blackboard.Models;

    using System.Collections.Generic;

    [GenerateSerializer]
    internal record struct BlackboardRegistryStateSurrogate(IReadOnlyCollection<BlackboardId> BoardIds);

    [RegisterConverter]
    internal sealed class BlackboardRegistryStateConverter : IConverter<BlackboardRegistryState, BlackboardRegistryStateSurrogate>
    {
        /// <inheritdoc />
        public BlackboardRegistryState ConvertFromSurrogate(in BlackboardRegistryStateSurrogate surrogate)
        {
            return new BlackboardRegistryState(surrogate.BoardIds);
        }

        /// <inheritdoc />
        public BlackboardRegistryStateSurrogate ConvertToSurrogate(in BlackboardRegistryState value)
        {
            return value.ToSurrogate();
        }
    }
}
