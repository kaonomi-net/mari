// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions.Exceptions;
    using MARI.Framework.Node.Blackboard.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when access a blackboard by a direct uid missing
    /// </summary>
    /// <seealso cref="MARIBaseException{BlackboardMissingMARIException}" />
    public sealed class BlackboardMissingMARIException : MARIBaseException<BlackboardMissingMARIException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardMissingMARIException"/> class.
        /// </summary>
        public BlackboardMissingMARIException(Guid? uid,
                                                   string? blackboardName,
                                                   string? blackboardTemplate,
                                                   Exception? innerException = null)
            : this(uid,
                   blackboardName,
                   blackboardTemplate,
                   BlackboardErrorSR.BlackboardMissingMARIException.WithArguments(uid, blackboardName, blackboardTemplate),
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.VGrain, genericType: MARIErrorCodes.ErrorType.Missing),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardMissingMARIException"/> class.
        /// </summary>
        internal BlackboardMissingMARIException(Guid? searchUid,
                                                     string? blackboardName,
                                                     string? blackboardTemplate,
                                                     string message,
                                                     ulong errorCode,
                                                     Exception? innerException)
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(BlackboardMissingMARIExceptionSurrogate.SearchUid), searchUid);
            this.Data.Add(nameof(BlackboardMissingMARIExceptionSurrogate.BlackboardName), blackboardName);
            this.Data.Add(nameof(BlackboardMissingMARIExceptionSurrogate.BlackboardTemplate), blackboardTemplate);
        }
    }

    [GenerateSerializer]
    public record struct BlackboardMissingMARIExceptionSurrogate(Guid? SearchUid,
                                                                      string? BlackboardName,
                                                                      string? BlackboardTemplate,
                                                                      string Message,
                                                                      ulong ErrorCode,
                                                                      Exception? InnerException) : IMARIBaseExceptionSurrogate;

    [RegisterConverter]
    public sealed class BlackboardMissingMARIExceptionConverter : IConverter<BlackboardMissingMARIException, BlackboardMissingMARIExceptionSurrogate>
    {
        /// <inheritdoc />
        public BlackboardMissingMARIException ConvertFromSurrogate(in BlackboardMissingMARIExceptionSurrogate surrogate)
        {
            return new BlackboardMissingMARIException(surrogate.SearchUid,
                                                           surrogate.BlackboardName,
                                                           surrogate.BlackboardTemplate,
                                                           surrogate.Message,
                                                           surrogate.ErrorCode,
                                                           surrogate.InnerException);
        }

        /// <inheritdoc />
        public BlackboardMissingMARIExceptionSurrogate ConvertToSurrogate(in BlackboardMissingMARIException value)
        {
            return new BlackboardMissingMARIExceptionSurrogate()
            {
                ErrorCode = value.ErrorCode,
                Message = value.Message,
                InnerException = value.InnerException,
                SearchUid = (Guid?)value.Data[nameof(BlackboardMissingMARIExceptionSurrogate.SearchUid)]!,
                BlackboardName = (string?)value.Data[nameof(BlackboardMissingMARIExceptionSurrogate.BlackboardName)]!,
                BlackboardTemplate = (string?)value.Data[nameof(BlackboardMissingMARIExceptionSurrogate.BlackboardTemplate)]!
            };
        }
    }
}
