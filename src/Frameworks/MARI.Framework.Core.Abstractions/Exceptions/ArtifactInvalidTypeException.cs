// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Resources;
    using MARI.Toolbox.Models;

    using System;

    /// <summary>
    /// Raised when an artefact is of wrong type
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public sealed class ArtifactInvalidTypeException : MARIExecutionBaseException<ArtifactInvalidTypeException>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactInvalidTypeException"/> class.
        /// </summary>
        public ArtifactInvalidTypeException(Guid artifactId,
                                            Type artifactType,
                                            Type expectedType,
                                            IExecutionContext executionContext,
                                            Exception? innerException = null)
            : this(MARIExceptionSR.ArtifactWrongType.WithArguments(artifactId, artifactType, expectedType),
                   executionContext.FlowUID,
                   executionContext.ParentExecutionId,
                   executionContext.CurrentExecutionId,
                   artifactId,
                   artifactType.GetAbstractType(),
                   expectedType.GetAbstractType(),
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Artifact,
                                             MARIErrorCodes.PartType.Type,
                                             MARIErrorCodes.ErrorType.Invalid),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactInvalidTypeException"/> class.
        /// </summary>
        internal ArtifactInvalidTypeException(string message,
                                              Guid flowUID,
                                              Guid? parentExecutionId,
                                              Guid currentExecutionId,
                                              Guid artifactId,
                                              AbstractType artifactType,
                                              AbstractType expectedType,
                                              ulong errorCode,
                                              Exception? innerException)
            : base(message, flowUID, parentExecutionId, currentExecutionId, errorCode, innerException)
        {
            base.Data.Add(nameof(ArtifactInvalidTypeExceptionSurrogate.ArtifactId), artifactId);
            base.Data.Add(nameof(ArtifactInvalidTypeExceptionSurrogate.ArtifactType), artifactType);
            base.Data.Add(nameof(ArtifactInvalidTypeExceptionSurrogate.ExpectedType), expectedType);
        }

        #endregion
    }

    [GenerateSerializer]
    public struct ArtifactInvalidTypeExceptionSurrogate : IMARIBaseExceptionSurrogate
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
        public AbstractType ArtifactType { get; set; }

        [Id(6)]
        public AbstractType ExpectedType { get; set; }

        [Id(7)]
        public Exception? InnerException { get; set; }

        [Id(8)]
        public Guid ArtifactId { get; set; }
    }

    [RegisterConverter]
    public sealed class ArtifactInvalidTypeExceptionConverter : IConverter<ArtifactInvalidTypeException, ArtifactInvalidTypeExceptionSurrogate>
    {
        public ArtifactInvalidTypeException ConvertFromSurrogate(in ArtifactInvalidTypeExceptionSurrogate surrogate)
        {
            return new ArtifactInvalidTypeException(surrogate.Message,
                                                    surrogate.FlowUid,
                                                    surrogate.ParentExecutionId,
                                                    surrogate.CurrentExecutionId,
                                                    surrogate.ArtifactId,
                                                    surrogate.ArtifactType,
                                                    surrogate.ExpectedType,
                                                    surrogate.ErrorCode,
                                                    surrogate.InnerException);
        }

        public ArtifactInvalidTypeExceptionSurrogate ConvertToSurrogate(in ArtifactInvalidTypeException value)
        {
            return new ArtifactInvalidTypeExceptionSurrogate()
            {
                Message = value.Message,
                InnerException = value.InnerException,
                ErrorCode = value.ErrorCode,
                FlowUid = (Guid)value.Data[nameof(IExecutionContext.FlowUID)]!,
                ParentExecutionId = (Guid?)value.Data[nameof(IExecutionContext.ParentExecutionId)],
                CurrentExecutionId = (Guid)value.Data[nameof(IExecutionContext.CurrentExecutionId)]!,

                ArtifactId = (Guid)value.Data[nameof(ArtifactInvalidTypeExceptionSurrogate.ArtifactId)]!,
                ArtifactType = (AbstractType)value.Data[nameof(ArtifactInvalidTypeExceptionSurrogate.ArtifactType)]!,
                ExpectedType = (AbstractType)value.Data[nameof(ArtifactInvalidTypeExceptionSurrogate.ExpectedType)]!,
            };
        }
    }
}
