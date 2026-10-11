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
    /// Raised when two entry conflict in the blackoard
    /// </summary>
    /// <seealso cref="MARIBaseException{BlackboardPushConflictException}" />
    public sealed class BlackboardPushValidationException : MARIBaseException<BlackboardPushValidationException>
    {
        public BlackboardPushValidationException(DataRecordContainer input,
                                                 string reason,
                                                 Exception? innerException)
            : this(BlackboardErrorSR.BlackboardPushValidationException.Replace("\\n", Environment.NewLine).WithArguments(reason, input.ToDebugDisplayName()),
                   JsonSerializer.Serialize(input),
                   reason,
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Data,
                                             MARIErrorCodes.PartType.Insert,
                                             MARIErrorCodes.ErrorType.Conflict),
                   innerException)
        {
        }

        internal BlackboardPushValidationException(string message,
                                                   string inputJson,
                                                   string reason,
                                                   ulong errorCode,
                                                   Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(BlackboardPushValidationExceptionSurrogate.InputJson), inputJson);
            this.Data.Add(nameof(BlackboardPushValidationExceptionSurrogate.Reason), reason);
        }
    }

    [GenerateSerializer]
    public record struct BlackboardPushValidationExceptionSurrogate(string Message,
                                                                    string InputJson,
                                                                    string Reason,
                                                                    ulong ErrorCode,
                                                                    Exception? InnerException) : IMARIBaseExceptionSurrogate;

    [RegisterConverter]
    public sealed class BlackboardPushValidationExceptionConverter : IConverter<BlackboardPushValidationException, BlackboardPushValidationExceptionSurrogate>
    {
        /// <inheritdoc />
        public BlackboardPushValidationException ConvertFromSurrogate(in BlackboardPushValidationExceptionSurrogate surrogate)
        {
            return new BlackboardPushValidationException(surrogate.Message,
                                                         surrogate.InputJson,
                                                         surrogate.Reason,
                                                         surrogate.ErrorCode,
                                                         surrogate.InnerException);
        }

        /// <inheritdoc />
        public BlackboardPushValidationExceptionSurrogate ConvertToSurrogate(in BlackboardPushValidationException value)
        {
            return new BlackboardPushValidationExceptionSurrogate()
            {
                ErrorCode = value.ErrorCode,
                Message = value.Message,
                InnerException = value.InnerException,
                InputJson = (string)value.Data[nameof(BlackboardPushValidationExceptionSurrogate.InputJson)]!,
                Reason = (string)value.Data[nameof(BlackboardPushValidationExceptionSurrogate.Reason)]!
            };
        }
    }
}
