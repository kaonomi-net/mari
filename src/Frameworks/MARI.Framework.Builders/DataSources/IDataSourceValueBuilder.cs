// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.DataSources
{
    using MARI.Framework.Core.Abstractions.Inputs;

    /// <summary>
    /// Data source use a simple value for exepression, fix, convertion ...
    /// </summary>
    /// <typeparam name="TOutputData">The type of the data source output.</typeparam>
    /// <seealso cref="IDefinitionBaseBuilder{DataSourceDefinition}" />
    public interface IDataSourceValueBuilder<TOutputData> : IDefinitionBaseBuilder<DataSourceDefinition>
    {
    }
}
