// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using System;

    /// <summary>
    /// Raised when definition required is missing
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public sealed class MARIException : MARIBaseException<MARIException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MARIException"/> class.
        /// </summary>
        public MARIException(string message,
                                  ulong errorCode = 0,
                                  Exception? innerException = null) 
            : base(message, errorCode, innerException)
        {
        }
    }

    [GenerateSerializer]
    public record struct MARIExceptionSurrogate(string Message,
                                                     ulong ErrorCode,
                                                     Exception? InnerException) : IMARIBaseExceptionSurrogate;

    [RegisterConverter]
    public sealed class MARIExceptionConverter : IConverter<MARIException, MARIExceptionSurrogate>
    {
        public MARIException ConvertFromSurrogate(in MARIExceptionSurrogate surrogate)
        {
            return new MARIException(surrogate.Message,
                                          surrogate.ErrorCode,
                                          surrogate.InnerException);
        }

        public MARIExceptionSurrogate ConvertToSurrogate(in MARIException value)
        {
            return new MARIExceptionSurrogate()
            {
                InnerException = value.InnerException,
                Message = value.Message,
                ErrorCode = value.ErrorCode,
            };
        }
    }
}
