// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions.Resources;

    using Orleans.Runtime;

    using System;

    /// <summary>
    /// Raised vgrain id is not the one expected
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public sealed class InvalidVGrainIdException : MARIBaseException<InvalidVGrainIdException>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidVGrainIdException"/> class.
        /// </summary>
        public InvalidVGrainIdException(GrainId receivedId, string expectDetails, Exception? innerException = null)
            : this(MARIExceptionSR.InvalidVGrainIdExceptionMessage.WithArguments(receivedId, expectDetails),
                   receivedId,
                   expectDetails,
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.VGrain, MARIErrorCodes.PartType.Identifier, MARIErrorCodes.ErrorType.Invalid),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidVGrainIdException"/> class.
        /// </summary>
        internal InvalidVGrainIdException(string message,
                                          GrainId receivedId,
                                          string expectDetails,
                                          ulong errorCode,
                                          Exception? innerException)
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(InvalidVGrainIdExceptionSurrogate.ReceivedId), receivedId);
            this.Data.Add(nameof(InvalidVGrainIdExceptionSurrogate.ExpectDetails), expectDetails);
        }

        #endregion
    }

    [GenerateSerializer]
    public struct InvalidVGrainIdExceptionSurrogate : IMARIBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public GrainId ReceivedId { get; set; }

        [Id(3)]
        public string ExpectDetails { get; set; }

        [Id(4)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class InvalidVGrainIdExceptionConverter : IConverter<InvalidVGrainIdException, InvalidVGrainIdExceptionSurrogate>
    {
        public InvalidVGrainIdException ConvertFromSurrogate(in InvalidVGrainIdExceptionSurrogate surrogate)
        {
            return new InvalidVGrainIdException(surrogate.Message,
                                                surrogate.ReceivedId,
                                                surrogate.ExpectDetails,
                                                surrogate.ErrorCode,
                                                surrogate.InnerException);
        }

        public InvalidVGrainIdExceptionSurrogate ConvertToSurrogate(in InvalidVGrainIdException value)
        {
            return new InvalidVGrainIdExceptionSurrogate()
            {
                InnerException = value.InnerException,
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                ReceivedId = (GrainId)value.Data[nameof(InvalidVGrainIdExceptionSurrogate.ReceivedId)]!,
                ExpectDetails = (string)value.Data[nameof(InvalidVGrainIdExceptionSurrogate.ExpectDetails)]!
            };
        }
    }
}
