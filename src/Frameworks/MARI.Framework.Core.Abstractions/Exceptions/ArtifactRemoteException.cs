// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when external artifact remote communication failed
    /// </summary>
    /// <seealso cref="MARIExecutionBaseException{ArtifactRemoteException}" />
    public sealed class ArtifactRemoteException : MARIExecutionBaseException<ArtifactRemoteException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactRemoteException"/> class.
        /// </summary>
        public ArtifactRemoteException(Guid artifactUid,
                                        string? details,     
                                        IExecutionContext executionContext,
                                        Exception? innerException = null) 
            : this(MARIExceptionSR.ArtifactRemoteException.WithArguments(artifactUid, details),
                   artifactUid,
                   executionContext.FlowUID,
                   executionContext.ParentExecutionId,
                   executionContext.CurrentExecutionId,
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Artifact, MARIErrorCodes.PartType.Execution),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactRemoteException"/> class.
        /// </summary>
        internal ArtifactRemoteException(string message,
                                          Guid artifactUid,
                                          Guid flowUid,
                                          Guid? parentExecutionId,
                                          Guid currentExecutionId,
                                          ulong errorCode,
                                          Exception? innerException = null)
            : base(message, flowUid, parentExecutionId, currentExecutionId, errorCode, innerException)
        {
            this.Data[nameof(ArtifactRemoteExceptionSurrogate.ArtifactUid)] = artifactUid;
        }
    }

    [GenerateSerializer]
    public struct ArtifactRemoteExceptionSurrogate : IMARIBaseExceptionSurrogate
    {
        public string Message { get; set; }

        public ulong ErrorCode { get; set; }

        public Exception? InnerException { get; set; }

        public Guid ArtifactUid { get; set; }
        
        public Guid FlowUid { get; set; }

        public Guid? ParentExecutionId { get; set; }

        public Guid CurrentExecutionId { get; set; }
    }

    [RegisterConverter]
    public sealed class ArtifactRemoteExceptionConverter : IConverter<ArtifactRemoteException, ArtifactRemoteExceptionSurrogate>
    {
        public ArtifactRemoteException ConvertFromSurrogate(in ArtifactRemoteExceptionSurrogate surrogate)
        {
            return new ArtifactRemoteException(surrogate.Message,
                                                surrogate.ArtifactUid,
                                                surrogate.FlowUid,
                                                surrogate.ParentExecutionId,
                                                surrogate.CurrentExecutionId,
                                                surrogate.ErrorCode,
                                                surrogate.InnerException);
        }

        public ArtifactRemoteExceptionSurrogate ConvertToSurrogate(in ArtifactRemoteException value)
        {
            return new ArtifactRemoteExceptionSurrogate()
            {
                Message = value.Message,
                InnerException = value.InnerException,
                ErrorCode = value.ErrorCode,
                FlowUid = (Guid)value.Data[nameof(IExecutionContext.FlowUID)]!,
                ParentExecutionId = (Guid?)value.Data[nameof(IExecutionContext.ParentExecutionId)],
                CurrentExecutionId = (Guid)value.Data[nameof(IExecutionContext.CurrentExecutionId)]!,
                ArtifactUid = (Guid)value.Data[nameof(ArtifactRemoteExceptionSurrogate.ArtifactUid)]!,
            };
        }
    }
}
