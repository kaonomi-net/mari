// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions.Resources;
    using MARI.Toolbox.Models;

    using System;

    /// <summary>
    /// Raised when definition required is missing
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public sealed class MissingDefinitionException : MARIBaseException<MissingDefinitionException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingDefinitionException"/> class.
        /// </summary>
        public MissingDefinitionException(Type definitionType,
                                          string definitionId,
                                          string? extraInfo = null,
                                          Exception? innerException = null)
            : this(MARIExceptionSR.DefinitionMissingExceptionMessage.WithArguments(definitionType, definitionId) + (string.IsNullOrEmpty(extraInfo) ? "" : " : " + extraInfo),
                   definitionType.GetAbstractType(),
                   definitionId,
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Definition, MARIErrorCodes.PartType.Identifier, MARIErrorCodes.ErrorType.Missing),
                   innerException)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingDefinitionException"/> class.
        /// </summary>
        internal MissingDefinitionException(string message,
                                            AbstractType definitionType,
                                            string definitionId,
                                            ulong errorCode,
                                            Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(MissingDefinitionExceptionSurrogate.DefinitionType), definitionType);
            this.Data.Add(nameof(MissingDefinitionExceptionSurrogate.DefinitionId), definitionId);
        }
    }

    [GenerateSerializer]
    public struct MissingDefinitionExceptionSurrogate : IMARIBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public ulong ErrorCode { get; set; }

        [Id(2)]
        public AbstractType DefinitionType { get; set; }

        [Id(3)]
        public string DefinitionId { get; set; }

        [Id(4)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class MissingDefinitionExceptionConverter : IConverter<MissingDefinitionException, MissingDefinitionExceptionSurrogate>
    {
        public MissingDefinitionException ConvertFromSurrogate(in MissingDefinitionExceptionSurrogate surrogate)
        {
            return new MissingDefinitionException(surrogate.Message,
                                                  surrogate.DefinitionType,
                                                  surrogate.DefinitionId,
                                                  surrogate.ErrorCode,
                                                  surrogate.InnerException);
        }

        public MissingDefinitionExceptionSurrogate ConvertToSurrogate(in MissingDefinitionException value)
        {
            return new MissingDefinitionExceptionSurrogate()
            {
                InnerException = value.InnerException,
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                DefinitionType = (AbstractType)value.Data[nameof(MissingDefinitionExceptionSurrogate.DefinitionType)]!,
                DefinitionId = (string)value.Data[nameof(MissingDefinitionExceptionSurrogate.DefinitionId)]!
            };
        }
    }
}
