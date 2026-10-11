// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions.Resources;
    using MARI.Toolbox.Models;

    using System;

    /// <summary>
    /// Raised when the input provide is not of the expected type
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public sealed class InvalidInputMARIException : MARIBaseException<InvalidInputMARIException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInputMARIException"/> class.
        /// </summary>
        /// <param name="inputType">ArtifactType of the input provide.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="executionInformation">The execution source information to understand when this occured, sequence input, stage, ...</param>
        public InvalidInputMARIException(Type inputType,
                                              Type expectedType,
                                              string? executionInformation = null,
                                              Exception? inner = null)

            : this(MARIExceptionSR.InvalidInputMARIExceptionMessage
                                       .WithArguments(inputType,
                                                      expectedType,
                                                      executionInformation ?? string.Empty),

                   inputType.GetAbstractType(),
                   expectedType.GetAbstractType(),
                   executionInformation,

                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Sequence,
                                             MARIErrorCodes.PartType.Input,
                                             MARIErrorCodes.ErrorType.Invalid,
                                             0),
                   inner)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInputMARIException"/> class.
        /// </summary>
        internal InvalidInputMARIException(string message,
                                                AbstractType inputType,
                                                AbstractType expectedType,
                                                string? executionInformation,
                                                ulong errorCode,
                                                Exception? innerException)
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(InvalidInputMARIExceptionSurrogate.InputType), inputType);
            this.Data.Add(nameof(InvalidInputMARIExceptionSurrogate.ExpectedType), expectedType);
            this.Data.Add(nameof(InvalidInputMARIExceptionSurrogate.ExecutionInformation), executionInformation ?? string.Empty);
        }
    }

    [GenerateSerializer]
    public struct InvalidInputMARIExceptionSurrogate : IMARIBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public AbstractType ExpectedType { get; set; }

        [Id(3)]
        public AbstractType InputType { get; set; }

        [Id(4)]
        public string ExecutionInformation { get; set; }

        [Id(5)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class InvalidInputMARIExceptionConverter : IConverter<InvalidInputMARIException, InvalidInputMARIExceptionSurrogate>
    {
        public InvalidInputMARIException ConvertFromSurrogate(in InvalidInputMARIExceptionSurrogate surrogate)
        {
            return new InvalidInputMARIException(surrogate.Message,
                                                      surrogate.InputType,
                                                      surrogate.ExpectedType,
                                                      surrogate.ExecutionInformation,
                                                      surrogate.ErrorCode,
                                                      surrogate.InnerException);
        }

        public InvalidInputMARIExceptionSurrogate ConvertToSurrogate(in InvalidInputMARIException value)
        {
            return new InvalidInputMARIExceptionSurrogate()
            {
                InnerException = value.InnerException,
                ErrorCode = value.ErrorCode,
                Message = value.Message,
                InputType = (AbstractType)value.Data[nameof(InvalidInputMARIExceptionSurrogate.InputType)]!,
                ExpectedType = (AbstractType)value.Data[nameof(InvalidInputMARIExceptionSurrogate.ExpectedType)]!,
                ExecutionInformation = (string)value.Data[nameof(InvalidOutputMARIExceptionSurrogate.ExecutionInformation)]!
            };
        }
    }
}
