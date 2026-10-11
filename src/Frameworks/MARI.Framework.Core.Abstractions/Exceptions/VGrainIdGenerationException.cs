// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions.Resources;
    using MARI.Toolbox.Models;

    using System;

    /// <summary>
    /// Raised when an vgrain if failed to be generated
    /// </summary>
    public sealed class VGrainIdGenerationException : MARIBaseException<VGrainIdGenerationException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainIdGenerationException"/> class.
        /// </summary>
        public VGrainIdGenerationException(Type vgrainType, Exception? innerException = null)
            : this(MARIExceptionSR.VGrainIdGenerationExceptionMessage.WithArguments(vgrainType),
                   vgrainType.GetAbstractType(),
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.VGrain, MARIErrorCodes.PartType.Identifier),
                   innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainIdGenerationException"/> class.
        /// </summary>
        internal VGrainIdGenerationException(string message,
                                             AbstractType vgrainType,
                                             ulong errorCode,
                                             Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(VGrainIdGenerationExceptionSurrogate.VGrainType), vgrainType);
        }
    }

    [GenerateSerializer]
    public struct VGrainIdGenerationExceptionSurrogate : IMARIBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public Exception? InnerException { get; set; }

        [Id(3)]
        public AbstractType VGrainType { get; set; }
    }

    [RegisterConverter]
    public sealed class VGrainIdGenerationExceptionConverter : IConverter<VGrainIdGenerationException, VGrainIdGenerationExceptionSurrogate>
    {
        public VGrainIdGenerationException ConvertFromSurrogate(in VGrainIdGenerationExceptionSurrogate surrogate)
        {
            return new VGrainIdGenerationException(surrogate.Message,
                                                   surrogate.VGrainType,
                                                   surrogate.ErrorCode,
                                                   surrogate.InnerException);
        }

        public VGrainIdGenerationExceptionSurrogate ConvertToSurrogate(in VGrainIdGenerationException value)
        {
            return new VGrainIdGenerationExceptionSurrogate()
            {
                InnerException = value.InnerException,
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                VGrainType = (AbstractType)value.Data[nameof(VGrainIdGenerationExceptionSurrogate.VGrainType)]!
            };
        }
    }
}
