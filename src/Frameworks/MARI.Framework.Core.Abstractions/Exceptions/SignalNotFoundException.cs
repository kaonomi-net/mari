// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when signal name provide is missing from the <see cref="ISignalDefinitionProvider"/>
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public sealed class SignalNotFoundException : MARIBaseException<SignalNotFoundException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignalNotFoundException"/> class.
        /// </summary>
        public SignalNotFoundException(string signalName, Exception? innerException = null)
            : this(MARIExceptionSR.SignalNotFounded.WithArguments(signalName),
                   signalName,
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Signal, MARIErrorCodes.PartType.Definition, MARIErrorCodes.ErrorType.NotFounded),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalNotFoundException"/> class.
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        [System.Text.Json.Serialization.JsonConstructor]
        public SignalNotFoundException(string message,
                                       string signalName,
                                       ulong errorCode,
                                       Exception? innerException)
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(SignalNotFoundExceptionSurrogate.SignalName), signalName);
        }
    }

    [GenerateSerializer]
    public record struct SignalNotFoundExceptionSurrogate(string Message,
                                                          ulong ErrorCode,
                                                          string SignalName,
                                                          Exception? InnerException) : IMARIBaseExceptionSurrogate;

    [RegisterConverter]
    public sealed class SignalNotFoundExceptionConverter : IConverter<SignalNotFoundException, SignalNotFoundExceptionSurrogate>
    {
        public SignalNotFoundException ConvertFromSurrogate(in SignalNotFoundExceptionSurrogate surrogate)
        {
           return new SignalNotFoundException(surrogate.Message, surrogate.SignalName, surrogate.ErrorCode, surrogate.InnerException);
        }

        public SignalNotFoundExceptionSurrogate ConvertToSurrogate(in SignalNotFoundException value)
        {
            return new SignalNotFoundExceptionSurrogate()
            {
                ErrorCode = value.ErrorCode,
                Message = value.Message,
                InnerException = value.InnerException,
                SignalName = (string)value.Data[nameof(SignalNotFoundExceptionSurrogate.SignalName)]!
            };
        }
    }
}
