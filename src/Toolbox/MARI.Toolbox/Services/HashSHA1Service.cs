// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Services
{
    using MARI.Toolbox.Abstractions.Services;

    using System.Security.Cryptography;

    /// <summary>
    /// Service in charge to provide SHA1 hashage 
    /// </summary>
    /// <seealso cref="IHashService" />
    public class HashSHA1Service : HashBaseService
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="HashSHA1Service"/> class.
        /// </summary>
        static HashSHA1Service()
        {
            Instance = new HashSHA1Service();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashSHA1Service"/> class.
        /// </summary>
        public HashSHA1Service()
            : base(SHA1.Create)
        {

        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default instance.
        /// </summary>
        public static HashSHA1Service Instance { get; }

        #endregion
    }
}
