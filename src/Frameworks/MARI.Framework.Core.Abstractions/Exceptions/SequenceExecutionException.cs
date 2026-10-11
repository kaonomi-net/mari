// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using System;

    /// <summary>
    /// Raised when an unespected exception occured during <see cref="ISequenceExcutor"/> execution
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public sealed class SequenceExecutionException : MARIBaseException<SequenceExecutionException>
    {
        public SequenceExecutionException(string message, Exception? innerException = null)
            : base(message,
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Sequence, MARIErrorCodes.PartType.Execution),
                   innerException)
        {
        }
    }

    [GenerateSerializer]
    public struct SequenceExecutionExceptionSurrogate : IMARIBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class SequenceExecutionExceptionConverter : IConverter<SequenceExecutionException, SequenceExecutionExceptionSurrogate>
    {
        public SequenceExecutionException ConvertFromSurrogate(in SequenceExecutionExceptionSurrogate surrogate)
        {
            return new SequenceExecutionException(surrogate.Message, surrogate.InnerException);
        }

        public SequenceExecutionExceptionSurrogate ConvertToSurrogate(in SequenceExecutionException value)
        {
            return new SequenceExecutionExceptionSurrogate()
            {
                Message = value.Message,
                InnerException = value.InnerException,
                ErrorCode = value.ErrorCode
            };
        }
    }
}
