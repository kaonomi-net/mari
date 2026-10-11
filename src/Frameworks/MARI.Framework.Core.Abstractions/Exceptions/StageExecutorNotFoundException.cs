// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions.Enums;
    using MARI.Framework.Core.Abstractions.Resources;
    using MARI.Framework.Core.Abstractions.Sequence;

    using System;

    /// <summary>
    /// Raised when door information provide is missing from the <see cref="IDoorDefinitionProvider"/>
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public sealed class StageExecutorNotFoundException : MARIBaseException<StageExecutorNotFoundException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StageExecutorNotFoundException"/> class.
        /// </summary>
        public StageExecutorNotFoundException(SequenceStageDefinition stageDefinition, Exception? innerException = null)
            : this(MARIExceptionSR.StageExecutorNotFoundException.WithArguments(stageDefinition.Type),
                   stageDefinition.Type,
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Door, MARIErrorCodes.PartType.Definition, MARIErrorCodes.ErrorType.NotFounded),
                   innerException)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StageExecutorNotFoundException"/> class.
        /// </summary>
        internal StageExecutorNotFoundException(string message,
                                                StageTypeEnum stageType,
                                                ulong errorCode,
                                                Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(StageExecutorNotFoundExceptionSurrogate.StageType), stageType);
        }
    }

    [GenerateSerializer]
    public record struct StageExecutorNotFoundExceptionSurrogate(string Message,
                                                                 StageTypeEnum StageType,
                                                                 ulong ErrorCode,
                                                                 Exception? InnerException) : IMARIBaseExceptionSurrogate;

    [RegisterConverter]
    public sealed class StageExecutorNotFoundExceptionConverter : IConverter<StageExecutorNotFoundException, StageExecutorNotFoundExceptionSurrogate>
    {
        public StageExecutorNotFoundException ConvertFromSurrogate(in StageExecutorNotFoundExceptionSurrogate surrogate)
        {
            return new StageExecutorNotFoundException(surrogate.Message,
                                                      surrogate.StageType,
                                                      surrogate.ErrorCode,
                                                      surrogate.InnerException);
        }

        public StageExecutorNotFoundExceptionSurrogate ConvertToSurrogate(in StageExecutorNotFoundException value)
        {
            return new StageExecutorNotFoundExceptionSurrogate()
            {
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                InnerException = value.InnerException,
                StageType = (StageTypeEnum)value.Data[nameof(StageExecutorNotFoundExceptionSurrogate.StageType)]!
            };
        }
    }
}
