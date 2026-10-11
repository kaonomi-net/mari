// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Cluster.Abstractions.Exceptions
{
    using MARI.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
    using MARI.Framework.Cluster.Abstractions.Resources;
    using MARI.Framework.Core.Abstractions.Exceptions;

    using System;

    /// <summary>
    /// Exception raised when the a required configuration is missing
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public sealed class MissingRequiredAutoMARIConfigurationException : MARIBaseException
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingRequiredAutoMARIConfigurationException"/> class.
        /// </summary>
        private MissingRequiredAutoMARIConfigurationException(Type autoConfiguratorType,
                                                                   string? autoPropKeyValue,
                                                                   Exception? innerException = null)
            : base(ErrorSR.RequiredAutoConfigurationMissing.WithArguments(autoConfiguratorType, autoPropKeyValue),
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Build, MARIErrorCodes.PartType.Configuration, MARIErrorCodes.ErrorType.Missing),
                   innerException)
        {
            this.Data.Add(nameof(autoConfiguratorType), autoConfiguratorType);
            this.Data.Add(nameof(autoPropKeyValue), autoPropKeyValue);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an exception <see cref="MissingRequiredMARIConfigurationException"/> with parameter rulled.
        /// </summary>
        public static MissingRequiredAutoMARIConfigurationException Create<TAutoConfigurator>(string? autoPropKeyValue)
            where TAutoConfigurator : IAutoConfigurator
        {
            return new MissingRequiredAutoMARIConfigurationException(typeof(TAutoConfigurator), autoPropKeyValue);
        }

        #endregion
    }
}
