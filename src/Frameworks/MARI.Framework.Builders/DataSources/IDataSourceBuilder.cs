// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.DataSources
{
    using MARI.Toolbox;

    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Build a data source provider
    /// </summary>
    /// <remarks>
    ///     Data source type builders are split in different interface to allow chosing the source allowed
    /// </remarks>
    public interface IDataSourceBuilder : IDataSourceBuilderWithStaticCollection,
                                          IDataSourceBuilderWithConvert,
                                          IDataSourceBuilderWithFixValue
    {

    }

    public interface IDataSourceBuilderWithStaticCollection
    {
        /// <summary>
        /// Get input from a serializable static collection.
        /// </summary>
        IDataSourceStaticCollectionBuilder<TData> StaticCollection<TData>(IEnumerable<TData> collection, Guid? fixUid = null);
    }

    public interface IDataSourceBuilderWithConvert
    {
        /// <summary>
        /// Get input from a serializable static collection.
        /// </summary>
        IDataSourceConvertValueBuilder<TInput, TData> Convert<TInput, TData>(Expression<Func<TInput, TData>> dataProvider, Guid? fixUid = null);
    }

    public interface IDataSourceBuilderWithFixValue
    {
        /// <summary>
        /// Get input from a serializable static collection.
        /// </summary>
        IDataSourceValueBuilder<TData> FixValue<TData>(TData data, Guid? fixUid = null);
    }
}
