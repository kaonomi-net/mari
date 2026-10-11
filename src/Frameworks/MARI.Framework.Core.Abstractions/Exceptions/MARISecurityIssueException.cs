// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions.Resources;

    using System;

    /// <summary>
    /// Rasied when access is not allowed
    /// </summary>
    /// <seealso cref="MARIBaseException{MARISecurityIssueException}" />
    public sealed class MARISecurityIssueException : MARIBaseException<MARISecurityIssueException>
    {
        #region Ctor

        public MARISecurityIssueException(string details,
                                               string right,
                                               Exception? innerException = null)
            : this(MARIExceptionSR.MARISecurityIssueExceptionMessage.WithArguments(right, details),
                   right,
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Security, MARIErrorCodes.PartType.Right, MARIErrorCodes.ErrorType.Failed),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MARISecurityIssueException"/> class.
        /// </summary>
        internal MARISecurityIssueException(string message,
                                               string right,
                                               ulong errorCode,
                                               Exception? innerException)
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(MARISecurityIssueExceptionSurrogate.Right), right);
        }

        #endregion
    }

    [GenerateSerializer]
    public record struct MARISecurityIssueExceptionSurrogate(string Message,
                                                                  string Right,
                                                                  ulong ErrorCode,
                                                                  Exception? InnerException) : IMARIBaseExceptionSurrogate;

    [RegisterConverter]
    public sealed class MARISecurityIssueExceptionConverter : IConverter<MARISecurityIssueException, MARISecurityIssueExceptionSurrogate>
    {
        /// <inheritdoc />
        public MARISecurityIssueException ConvertFromSurrogate(in MARISecurityIssueExceptionSurrogate surrogate)
        {
            return new MARISecurityIssueException(surrogate.Message, surrogate.Right, surrogate.ErrorCode, surrogate.InnerException);
        }

        /// <inheritdoc />
        public MARISecurityIssueExceptionSurrogate ConvertToSurrogate(in MARISecurityIssueException value)
        {
            return new MARISecurityIssueExceptionSurrogate()
            {
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                InnerException = value.InnerException,
                Right = (string)value.Data[nameof(MARISecurityIssueExceptionSurrogate.Right)]!
            };
        }
    }
}
