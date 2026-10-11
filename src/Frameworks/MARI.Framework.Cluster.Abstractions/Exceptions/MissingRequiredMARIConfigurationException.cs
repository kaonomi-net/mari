// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Cluster.Abstractions.Exceptions
{
    using MARI.Framework.Cluster.Abstractions.Resources;
    using MARI.Framework.Core.Abstractions.Exceptions;

    using System;

    /// <summary>
    /// Exception raised when the a required configuration is missing
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public sealed class MissingRequiredMARIConfigurationException : MARIBaseException
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingRequiredMARIConfigurationException"/> class.
        /// </summary>
        public MissingRequiredMARIConfigurationException(string configurationKey,
                                                              string configurationDetail,
                                                              Exception? innerException = null)
            : base(ErrorSR.RequiredAutoConfigurationMissing.WithArguments(configurationKey, configurationDetail),
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Build, MARIErrorCodes.PartType.Configuration, MARIErrorCodes.ErrorType.Missing),
                   innerException)
        {
            this.Data.Add(nameof(configurationKey), configurationKey);
            this.Data.Add(nameof(configurationDetail), configurationDetail);
        }

        #endregion
    }
}
