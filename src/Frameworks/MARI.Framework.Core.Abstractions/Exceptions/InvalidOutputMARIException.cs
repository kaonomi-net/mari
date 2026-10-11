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
    public sealed class InvalidOutputMARIException : MARIBaseException<InvalidOutputMARIException>
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidOutputMARIException"/> class.
        /// </summary>
        /// <param name="desiredType">ArtifactType of the output desired.</param>
        /// <param name="outputType">The execuption result provided.</param>
        /// <param name="executionInformation">The execution source information to understand when this occured, sequence input, stage, ...</param>
        public InvalidOutputMARIException(Type desiredType,
                                               Type outputType,
                                               string? executionInformation = null,
                                               Exception? inner = null)
            : this(MARIExceptionSR.InvalidOutputMARIExceptionMessage
                                       .WithArguments(desiredType,
                                                      outputType,
                                                      executionInformation ?? string.Empty),

                   desiredType.GetAbstractType(),
                   outputType.GetAbstractType(),
                   executionInformation,
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Sequence,
                                             MARIErrorCodes.PartType.Output,
                                             MARIErrorCodes.ErrorType.Invalid,
                                             0),
                   inner)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidOutputMARIException"/> class.
        /// </summary>
        internal InvalidOutputMARIException(string message,
                                                 AbstractType desiredType,
                                                 AbstractType outputType,
                                                 string? executionInformation,
                                                 ulong errorCode,
                                                 Exception? innerException)
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(InvalidOutputMARIExceptionSurrogate.DesiredType), desiredType);
            this.Data.Add(nameof(InvalidOutputMARIExceptionSurrogate.OutputType), outputType);
            this.Data.Add(nameof(InvalidOutputMARIExceptionSurrogate.ExecutionInformation), executionInformation ?? string.Empty);
        }
    }

    [GenerateSerializer]
    public struct InvalidOutputMARIExceptionSurrogate : IMARIBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public AbstractType DesiredType { get; set; }

        [Id(3)]
        public AbstractType OutputType { get; set; }

        [Id(4)]
        public string ExecutionInformation { get; set; }

        [Id(5)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class InvalidOutputMARIExceptionConverter : IConverter<InvalidOutputMARIException, InvalidOutputMARIExceptionSurrogate>
    {
        public InvalidOutputMARIException ConvertFromSurrogate(in InvalidOutputMARIExceptionSurrogate surrogate)
        {
            return new InvalidOutputMARIException(surrogate.Message,
                                                       surrogate.DesiredType,
                                                       surrogate.OutputType,
                                                       surrogate.ExecutionInformation,
                                                       surrogate.ErrorCode,
                                                       surrogate.InnerException);
        }

        public InvalidOutputMARIExceptionSurrogate ConvertToSurrogate(in InvalidOutputMARIException value)
        {
            return new InvalidOutputMARIExceptionSurrogate()
            {
                InnerException = value.InnerException,
                ErrorCode = value.ErrorCode,
                Message = value.Message,
                DesiredType = (AbstractType)value.Data[nameof(InvalidOutputMARIExceptionSurrogate.DesiredType)]!,
                OutputType = (AbstractType)value.Data[nameof(InvalidOutputMARIExceptionSurrogate.OutputType)]!,
                ExecutionInformation = (string)value.Data[nameof(InvalidOutputMARIExceptionSurrogate.ExecutionInformation)]!
            };
        }
    }
}
