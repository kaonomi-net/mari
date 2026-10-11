// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions.Resources;

    using Orleans;

    using System;

    /// <summary>
    /// Raised when store etag missmatch
    /// </summary>
    /// <seealso cref="MARIBaseException{MemoryRepositoryStorageEtagMismatchException}" />
    public sealed class MemoryRepositoryStorageEtagMismatchException : MARIBaseException<MemoryRepositoryStorageEtagMismatchException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryRepositoryStorageEtagMismatchException"/> class.
        /// </summary>
        public MemoryRepositoryStorageEtagMismatchException(string storedEtag,
                                                            string receivedEtag,
                                                            Exception? innerException = null)
            : this(storedEtag,
                   receivedEtag,
                   MARIExceptionSR.MemoryRepositoryStorageEtagMismatchExceptionMessage.WithArguments(storedEtag, receivedEtag),
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Storage, MARIErrorCodes.PartType.Etag, MARIErrorCodes.ErrorType.Mismatch),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryRepositoryStorageEtagMismatchException"/> class.
        /// </summary>
        internal MemoryRepositoryStorageEtagMismatchException(string storedEtag,
                                                              string receivedEtag,
                                                              string message,
                                                              ulong errorCode,
                                                              Exception? innerException = null)
            : base(message, errorCode, innerException)  
        {
            this.Data.Add(nameof(MemoryRepositoryStorageEtagMismatchExceptionSurrogate.StoredEtag), storedEtag);
            this.Data.Add(nameof(MemoryRepositoryStorageEtagMismatchExceptionSurrogate.ReceivedEtag), receivedEtag);
        }
    }

    [GenerateSerializer]
    public struct MemoryRepositoryStorageEtagMismatchExceptionSurrogate : IMARIBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public Exception? InnerException { get; set; }

        [Id(3)]
        public string StoredEtag { get; set; }

        [Id(4)]
        public string ReceivedEtag { get; set; }
    }

    [RegisterConverter]
    public sealed class MemoryRepositoryStorageEtagMismatchExceptionConverter : IConverter<MemoryRepositoryStorageEtagMismatchException, MemoryRepositoryStorageEtagMismatchExceptionSurrogate>
    {
        /// <inheritdoc />
        public MemoryRepositoryStorageEtagMismatchException ConvertFromSurrogate(in MemoryRepositoryStorageEtagMismatchExceptionSurrogate surrogate)
        {
            return new MemoryRepositoryStorageEtagMismatchException(surrogate.StoredEtag,
                                                                    surrogate.ReceivedEtag,
                                                                    surrogate.Message,
                                                                    surrogate.ErrorCode,
                                                                    surrogate.InnerException);
        }

        /// <inheritdoc />
        public MemoryRepositoryStorageEtagMismatchExceptionSurrogate ConvertToSurrogate(in MemoryRepositoryStorageEtagMismatchException value)
        {
            return new MemoryRepositoryStorageEtagMismatchExceptionSurrogate()
            {
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                InnerException = value.InnerException,
                StoredEtag = (string)value.Data[nameof(MemoryRepositoryStorageEtagMismatchExceptionSurrogate.StoredEtag)]!,
                ReceivedEtag = (string)value.Data[nameof(MemoryRepositoryStorageEtagMismatchExceptionSurrogate.ReceivedEtag)]!,
            };
        }
    }
}
