// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Services
{
    using MARI.Toolbox.Abstractions.Services;

    using System.Security.Cryptography;

    /// <summary>
    /// Service in charge to provide SHA256 hashage 
    /// </summary>
    /// <seealso cref="IHashService" />
    public class HashSHA256Service : HashBaseService
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="HashSHA256Service"/> class.
        /// </summary>
        static HashSHA256Service()
        {
            Instance = new HashSHA256Service();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashSHA256Service"/> class.
        /// </summary>
        public HashSHA256Service()
            : base(SHA256.Create)
        {

        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default instance.
        /// </summary>
        public static HashSHA256Service Instance { get; }

        #endregion
    }
}
