// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Exceptions;
    using MARI.Framework.Core.Abstractions.Resources;

    using Orleans;

    using System;

    /// <summary>
    /// Raised when <see cref="IExternalCodeExecutorPreparationStep"/> setup/unstep failed
    /// </summary>
    /// <seealso cref="MARIExecutionBaseException" />
    public sealed class ArtifactPreparationFailedException : MARIExecutionBaseException<ArtifactPreparationFailedException>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactPreparationFailedException"/> class.
        /// </summary>
        public ArtifactPreparationFailedException(string details,
                                                  IExecutionContext? executionContext,
                                                  Exception? innerException = null)
            : this(MARIExceptionSR.ArtifactPreparationFailed.WithArguments(details),
                   details?.ToString() ?? string.Empty,
                   executionContext?.FlowUID ?? Guid.Empty,
                   executionContext?.ParentExecutionId,
                   executionContext?.CurrentExecutionId ?? Guid.Empty,
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Artifact,
                                             MARIErrorCodes.PartType.Setup,
                                             MARIErrorCodes.ErrorType.Failed),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactPreparationFailedException"/> class.
        /// </summary>
        internal ArtifactPreparationFailedException(string message,
                                                    string details,
                                                    Guid flowUid,
                                                    Guid? parentExecutionId,
                                                    Guid currentExecutionId,
                                                    ulong errorCode,
                                                    Exception? innerException = null) 
            : base(message, flowUid, parentExecutionId, currentExecutionId, errorCode, innerException)
        {
            this.Data.Add(nameof(ArtifactPreparationFailedExceptionSurrogate.Details), details);
        }

        #endregion
    }

    [GenerateSerializer]
    public struct ArtifactPreparationFailedExceptionSurrogate : IMARIBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public Guid FlowUid { get; set; }

        [Id(3)]
        public Guid? ParentExecutionId { get; set; }

        [Id(4)]
        public Guid CurrentExecutionId { get; set; }

        [Id(5)]
        public Exception? InnerException { get; set; }

        [Id(6)]
        public string Details { get; set; }
    }

    [RegisterConverter]
    public sealed class ArtifactPreparationFailedExceptionConverter : IConverter<ArtifactPreparationFailedException, ArtifactPreparationFailedExceptionSurrogate>
    {
        public ArtifactPreparationFailedException ConvertFromSurrogate(in ArtifactPreparationFailedExceptionSurrogate surrogate)
        {
           return new ArtifactPreparationFailedException(surrogate.Message,
                                                         surrogate.Details,
                                                         surrogate.FlowUid,
                                                         surrogate.ParentExecutionId,
                                                         surrogate.CurrentExecutionId,
                                                         surrogate.ErrorCode,
                                                         surrogate.InnerException);
        }

        public ArtifactPreparationFailedExceptionSurrogate ConvertToSurrogate(in ArtifactPreparationFailedException value)
        {
            return new ArtifactPreparationFailedExceptionSurrogate()
            {
                Message = value.Message,
                InnerException = value.InnerException,
                ErrorCode = value.ErrorCode,
                Details = (string)value.Data[nameof(ArtifactPreparationFailedExceptionSurrogate.Details)]!,
                FlowUid = (Guid)value.Data[nameof(IExecutionContext.FlowUID)]!,
                ParentExecutionId = (Guid?)value.Data[nameof(IExecutionContext.ParentExecutionId)],
                CurrentExecutionId = (Guid)value.Data[nameof(IExecutionContext.CurrentExecutionId)]!,
            };
        }
    }
}
