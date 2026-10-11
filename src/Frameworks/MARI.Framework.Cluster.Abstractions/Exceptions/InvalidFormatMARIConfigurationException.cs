// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Cluster.Abstractions.Exceptions
{
    using MARI.Framework.Cluster.Abstractions.Resources;
    using MARI.Framework.Core.Abstractions.Exceptions;

    using System;

    /// <summary>
    /// Raised when a configuration provide is not in the expected format
    /// </summary>
    public sealed class InvalidFormatMARIConfigurationException : MARIBaseException
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidFormatMARIConfigurationException"/> class.
        /// </summary>
        public InvalidFormatMARIConfigurationException(string configurationProperty,
                                                            string configurationValue,
                                                            string expectedFormatDefinition,
                                                            Exception? innerException = null)
            : base(ErrorSR.InvalidFormatMARICondigurationMessage.WithArguments(configurationProperty, configurationValue, expectedFormatDefinition),
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Configuration, MARIErrorCodes.PartType.Property, MARIErrorCodes.ErrorType.Invalid),
                   innerException)
        {
            this.Data.Add(nameof(configurationProperty), configurationProperty);
            this.Data.Add(nameof(configurationValue), configurationValue);
            this.Data.Add(nameof(expectedFormatDefinition), expectedFormatDefinition);
        }

        #endregion
    }
}
