// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Inputs
{
    using MARI.Framework.Core.Abstractions.Inputs;

    /// <summary>
    /// Factory in change to provide a <see cref="IDataSourceProvider"/> by <see cref="DataSourceDefinition"/>
    /// </summary>
    public interface IDataSourceProviderFactory
    {
        /// <summary>
        /// Determines whether <paramref name="provider"/> is still a valid one for the input <paramref name="inputSourceDefinition"/>
        /// </summary>
        ValueTask<bool> IsStillValidAsync(IDataSourceProvider provider, DataSourceDefinition inputSourceDefinition, CancellationToken token = default);

        /// <summary>
        /// Gets a provider associate to <paramref name="inputSourceDefinition"/>
        /// </summary>
        IDataSourceProvider GetProvider(DataSourceDefinition inputSourceDefinition);
    }
}
