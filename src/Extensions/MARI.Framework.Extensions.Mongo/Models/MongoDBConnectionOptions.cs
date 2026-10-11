// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Extensions.Mongo.Models
{
    using Orleans.Providers.MongoDB.Configuration;

    /// <summary>
    /// Storage a mongo connection configuration attached to a specific key
    /// </summary>
    /// <seealso cref="MongoDBOptions" />
    internal sealed class MongoDBConnectionOptions : MongoDBOptions
    {
        #region Fields

        public const string MARI_CLUSTER = "Membership";
        public const string MARI_REMINDER = nameof(MARI_REMINDER);
        public const string MARI_DEFAULT = nameof(MARI_DEFAULT);
        public const string MARI_VGRAIN_STATE = nameof(MARI_VGRAIN_STATE);
        public const string MARI_SIGNAL_STATE = nameof(MARI_SIGNAL_STATE);
        public const string MARI_DOOR_STATE = nameof(MARI_DOOR_STATE);

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="MongoDBConnectionOptions"/> class.
        /// </summary>
        static MongoDBConnectionOptions()
        {
            Default = new MongoDBConnectionOptions()
            {
                ConnectionString = "mongodb://127.0.0.1:27017"
            };
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static MongoDBConnectionOptions Default { get; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public int? Order { get; set; }

        /// <summary>
        /// Gets the key.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string? ConnectionString { get; set; }

        #endregion
    }
}
