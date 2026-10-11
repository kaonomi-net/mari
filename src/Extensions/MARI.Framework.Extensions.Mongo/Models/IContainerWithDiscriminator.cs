// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Extensions.Mongo.Models
{
    using MongoDB.Driver;

    internal interface IContainerWithDiscriminator<TContainer>
    {
        /// <summary>
        /// Gets the discriminator filter.
        /// </summary>
        FilterDefinition<TContainer>? DiscriminatorFilter { get; }
    }
}
