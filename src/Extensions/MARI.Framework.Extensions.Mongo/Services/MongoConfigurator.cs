// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Extensions.Mongo.Services
{
    using MARI.Framework.Cluster.Abstractions.Exceptions;
    using MARI.Framework.Extensions.Mongo.Abstractions;
    using MARI.Framework.Extensions.Mongo.Models;

    using MARI.Toolbox.Extensions;

    using Microsoft.CodeAnalysis.Options;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    using MongoDB.Driver;

    using Orleans.Providers.MongoDB.Configuration;
    using Orleans.Providers.MongoDB.StorageProviders.Serializers;
    using Orleans.Providers.MongoDB.Utils;

    using System;

    /// <summary>
    /// Define configuration used by all mongo configuration
    /// </summary>
    public sealed class MongoConfigurator
    {
        #region Fields

        private const string URI_TYPE = "mongodb://";
        private const string SRV_URI_TYPE = "mongodb+srv://";

        #endregion

        #region Methods

        /// <summary>
        /// Normalizes the connection string.
        /// </summary>
        public static string CheckAndNormalizeConnectionString(string connectionString)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(connectionString);

            if (!connectionString.StartsWith(URI_TYPE, StringComparison.OrdinalIgnoreCase) &&
                !connectionString.StartsWith(SRV_URI_TYPE, StringComparison.OrdinalIgnoreCase))
            {
                connectionString = URI_TYPE + connectionString;
            }

            if (!Uri.TryCreate(connectionString, UriKind.Absolute, out _))
            {
                throw new InvalidFormatMARIConfigurationException("Mongo." + nameof(connectionString),
                                                                       connectionString,
                                                                       URI_TYPE + "...@HOST:PORT?OPTIONS");
            }

            return connectionString;
        }

        /// <summary>
        /// Maps option to another one, if null try using the default
        /// </summary>
        internal static TOption MapOption<TOption>(MongoDBOptions source, TOption destination)
            where TOption : MongoDBOptions
        {
            destination.ClientName = source?.ClientName;
            destination.CollectionConfigurator = source?.CollectionConfigurator;
            destination.CollectionPrefix = source?.CollectionPrefix;
            destination.CreateShardKeyForCosmos = source?.CreateShardKeyForCosmos ?? false;
            destination.DatabaseName = source?.DatabaseName;

            return destination;
        }

        #endregion
    }
}
