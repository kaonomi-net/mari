// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Exceptions;

    /// <summary>
    /// Raised when a not autorize action is done that MUST not arrived
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public sealed class VGrainInvalidOperationMARIException : MARIExecutionBaseException<VGrainInvalidOperationMARIException>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainMethodMARIException"/> class.
        /// </summary>
        public VGrainInvalidOperationMARIException(string message,
                                                        IExecutionContext executionContext,
                                                        Exception? innerException = null)
            : this(message,
                   executionContext.FlowUID,
                   executionContext.ParentExecutionId,
                   executionContext.CurrentExecutionId,
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Sequence, MARIErrorCodes.PartType.Execution, MARIErrorCodes.ErrorType.Invalid),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainInvalidOperationMARIException"/> class.
        /// </summary>
        internal VGrainInvalidOperationMARIException(string message,
                                                          Guid flowUid,
                                                          Guid? parentExecutionId,
                                                          Guid currentExecutionId,
                                                          ulong errorCode,
                                                          Exception? innerException = null)
            : base(message, flowUid, parentExecutionId, currentExecutionId, errorCode, innerException)
        {
        }

        #endregion
    }

    [GenerateSerializer]
    public struct VGrainInvalidOperationMARIExceptionSurrogate : IMARIBaseExceptionSurrogate
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
    }

    [RegisterConverter]
    public sealed class VGrainInvalidOperationMARIExceptionConverter : IConverter<VGrainInvalidOperationMARIException, VGrainInvalidOperationMARIExceptionSurrogate>
    {
        public VGrainInvalidOperationMARIException ConvertFromSurrogate(in VGrainInvalidOperationMARIExceptionSurrogate surrogate)
        {
            return new VGrainInvalidOperationMARIException(surrogate.Message,
                                                                surrogate.FlowUid,
                                                                surrogate.ParentExecutionId,
                                                                surrogate.CurrentExecutionId,
                                                                surrogate.ErrorCode,
                                                                surrogate.InnerException);
        }

        public VGrainInvalidOperationMARIExceptionSurrogate ConvertToSurrogate(in VGrainInvalidOperationMARIException value)
        {
            return new VGrainInvalidOperationMARIExceptionSurrogate()
            {
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                FlowUid = (Guid)value.Data[nameof(IExecutionContext.FlowUID)]!,
                ParentExecutionId = (Guid?)value.Data[nameof(IExecutionContext.ParentExecutionId)],
                CurrentExecutionId = (Guid)value.Data[nameof(IExecutionContext.CurrentExecutionId)]!,
                InnerException = value.InnerException
            };
        }
    }
}
