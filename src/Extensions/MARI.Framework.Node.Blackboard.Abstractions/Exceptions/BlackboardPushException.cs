// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions.Exceptions;
    using MARI.Framework.Node.Blackboard.Abstractions.Models;
    using MARI.Framework.Node.Blackboard.Abstractions.Resources;

    using System;
    using System.Text.Json;

    /// <summary>
    /// Raised when push data failed
    /// </summary>
    /// <seealso cref="MARIBaseException{BlackboardPushException}" />
    public sealed class BlackboardPushException : MARIBaseException<BlackboardPushException>
    {
        public BlackboardPushException(DataRecordContainer input,
                                                 string reason,
                                                 Exception? innerException)
            : this(BlackboardErrorSR.BlackboardPushException.WithArguments(reason, input.ToDebugDisplayName()),
                   JsonSerializer.Serialize(input),
                   reason,
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Data,
                                             MARIErrorCodes.PartType.Insert,
                                             MARIErrorCodes.ErrorType.Conflict),
                   innerException)
        {
        }

        internal BlackboardPushException(string message,
                                                   string inputJson,
                                                   string reason,
                                                   ulong errorCode,
                                                   Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(BlackboardPushExceptionSurrogate.InputJson), inputJson);
            this.Data.Add(nameof(BlackboardPushExceptionSurrogate.Reason), reason);
        }
    }

    [GenerateSerializer]
    public record struct BlackboardPushExceptionSurrogate(string Message,
                                                          string InputJson,
                                                          string Reason,
                                                          ulong ErrorCode,
                                                          Exception? InnerException) : IMARIBaseExceptionSurrogate;

    [RegisterConverter]
    public sealed class BlackboardPushExceptionConverter : IConverter<BlackboardPushException, BlackboardPushExceptionSurrogate>
    {
        /// <inheritdoc />
        public BlackboardPushException ConvertFromSurrogate(in BlackboardPushExceptionSurrogate surrogate)
        {
            return new BlackboardPushException(surrogate.Message,
                                                         surrogate.InputJson,
                                                         surrogate.Reason,
                                                         surrogate.ErrorCode,
                                                         surrogate.InnerException);
        }

        /// <inheritdoc />
        public BlackboardPushExceptionSurrogate ConvertToSurrogate(in BlackboardPushException value)
        {
            return new BlackboardPushExceptionSurrogate()
            {
                ErrorCode = value.ErrorCode,
                Message = value.Message,
                InnerException = value.InnerException,
                InputJson = (string)value.Data[nameof(BlackboardPushExceptionSurrogate.InputJson)]!,
                Reason = (string)value.Data[nameof(BlackboardPushExceptionSurrogate.Reason)]!
            };
        }
    }
}
