// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using System;

    /// <summary>
    /// Raised when a security feature have been broken
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public sealed class VGrainSecurityMARIException : MARIBaseException<VGrainSecurityMARIException>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainSecurityMARIException"/> class.
        /// </summary>
        public VGrainSecurityMARIException(string message, Exception? innerException = null)
            : base(message,
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Security),
                   innerException)
        {
        }

        #endregion
    }

    [GenerateSerializer]
    public struct VGrainSecurityMARIExceptionSurrogate : IMARIBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class VGrainSecurityMARIExceptionConverter : IConverter<VGrainSecurityMARIException, VGrainSecurityMARIExceptionSurrogate>
    {
        public VGrainSecurityMARIException ConvertFromSurrogate(in VGrainSecurityMARIExceptionSurrogate surrogate)
        {
            return new VGrainSecurityMARIException(surrogate.Message, surrogate.InnerException);
        }

        public VGrainSecurityMARIExceptionSurrogate ConvertToSurrogate(in VGrainSecurityMARIException value)
        {
            return new VGrainSecurityMARIExceptionSurrogate() 
            {
                Message = value.Message, 
                ErrorCode = value.ErrorCode,
                InnerException = value.InnerException 
            };
        }
    }
}
