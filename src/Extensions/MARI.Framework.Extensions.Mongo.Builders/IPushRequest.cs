// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Extensions.Mongo.Builders
{
    using MARI.Framework.Core.Abstractions;

    using System.Threading.Tasks;

    /// <summary>
    /// request used to push multiple definitions
    /// </summary>
    public interface IPushRequest
    {
        /// <summary>
        /// Appends the definition into the request
        /// </summary>
        IPushRequest AppendDefinition<TDefinition>(params TDefinition[] definitions)
             where TDefinition : class, IDefinition;

        /// <summary>
        /// Pushes defintions appended to the storages
        /// </summary>
        Task<bool> PushAsync(CancellationToken token);
    }
}
