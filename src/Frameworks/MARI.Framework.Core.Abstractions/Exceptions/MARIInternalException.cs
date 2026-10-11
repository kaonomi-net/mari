// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using MARI.Toolbox.Models;

    using System;

    /// <summary>
    /// Raised when definition required is missing
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public sealed class MARIInternalException : MARIBaseException<MARIInternalException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MARIInternalException"/> class.
        /// </summary>
        public MARIInternalException(MARIBaseException originException,
                                          Exception? innerException = null)
            : this(originException?.Message ?? "Relay internal exception",
                   originException?.GetType().GetAbstractType() ?? typeof(Exception).GetAbstractType(),
                   originException?.ErrorCode ?? MARIErrorCodes.Build(MARIErrorCodes.Categories.Execution, genericType: MARIErrorCodes.ErrorType.Failed),
                   innerException)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MARIInternalException"/> class.
        /// </summary>
        internal MARIInternalException(string message,
                                            AbstractType exceptionType,
                                            ulong errorCode,
                                            Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(MARIInternalExceptionSurrogate.ExceptionType), exceptionType);
        }
    }

    [GenerateSerializer]
    public record struct MARIInternalExceptionSurrogate(string Message,
                                                             ulong ErrorCode,
                                                             AbstractType ExceptionType,
                                                             Exception? InnerException) : IMARIBaseExceptionSurrogate;

    [RegisterConverter]
    public sealed class MARIInternalExceptionConverter : IConverter<MARIInternalException, MARIInternalExceptionSurrogate>
    {
        public MARIInternalException ConvertFromSurrogate(in MARIInternalExceptionSurrogate surrogate)
        {
            return new MARIInternalException(surrogate.Message,
                                                  surrogate.ExceptionType,
                                                  surrogate.ErrorCode,
                                                  surrogate.InnerException);
        }

        public MARIInternalExceptionSurrogate ConvertToSurrogate(in MARIInternalException value)
        {
            return new MARIInternalExceptionSurrogate()
            {
                InnerException = value.InnerException,
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                ExceptionType = (AbstractType)value.Data[nameof(MARIInternalExceptionSurrogate.ExceptionType)]!,
            };
        }
    }
}
