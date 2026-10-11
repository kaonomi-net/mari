// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.Exceptions
{
    using MARI.Framework.Builders.Resources;
    using MARI.Framework.Core.Abstractions.Exceptions;

    using System;

    /// <summary>
    /// Raised when the build parameters are invalid or missing 
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public sealed class InvalidParameterException : MARIBaseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidParameterException"/> class.
        /// </summary>
        public InvalidParameterException(string paramName,
                                         string message,
                                         Exception? innerException = null)
            : base(BuildErrorSR.InvalidParameterExceptionMessage.WithArguments(paramName, message),
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Build,
                                             MARIErrorCodes.PartType.Property,
                                             MARIErrorCodes.ErrorType.Invalid),
                   innerException)
        {
            this.Data.Add(nameof(paramName), paramName);
        }
    }
}
