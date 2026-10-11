// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions.Resources;
    using MARI.Toolbox.Models;

    using System;

    /// <summary>
    /// Raised when definition conflict with another one
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public sealed class ConflictDefinitionException : MARIBaseException<ConflictDefinitionException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictDefinitionException"/> class.
        /// </summary>
        public ConflictDefinitionException(Type definitionType,
                                           string definitionSource,
                                           string conflictDefinition,
                                           Exception? innerException = null)
            : this(MARIExceptionSR.ConflictDefinitionException.WithArguments(definitionType, definitionSource, conflictDefinition),
                   definitionType.GetAbstractType(),
                   definitionSource,
                   conflictDefinition,
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Definition, MARIErrorCodes.PartType.Identifier, MARIErrorCodes.ErrorType.Missing),
                   innerException)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConflictDefinitionException"/> class.
        /// </summary>
        internal ConflictDefinitionException(string message,
                                            AbstractType definitionType,
                                            string definitionSource,
                                            string conflictDefinition,
                                            ulong errorCode,
                                            Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(ConflictDefinitionExceptionSurrogate.DefinitionType), definitionType);
            this.Data.Add(nameof(ConflictDefinitionExceptionSurrogate.DefinitionSource), definitionSource);
            this.Data.Add(nameof(ConflictDefinitionExceptionSurrogate.ConflictDefinition), conflictDefinition);
        }
    }

    [GenerateSerializer]
    public record struct ConflictDefinitionExceptionSurrogate(string Message,
                                                              ulong ErrorCode,
                                                              Exception? InnerException,
                                                              AbstractType DefinitionType,
                                                              string DefinitionSource,
                                                              string ConflictDefinition) : IMARIBaseExceptionSurrogate;

    [RegisterConverter]
    public sealed class ConflictDefinitionExceptionConverter : IConverter<ConflictDefinitionException, ConflictDefinitionExceptionSurrogate>
    {
        public ConflictDefinitionException ConvertFromSurrogate(in ConflictDefinitionExceptionSurrogate surrogate)
        {
            return new ConflictDefinitionException(surrogate.Message,
                                                  surrogate.DefinitionType,
                                                  surrogate.DefinitionSource,
                                                  surrogate.ConflictDefinition,
                                                  surrogate.ErrorCode,
                                                  surrogate.InnerException);
        }

        public ConflictDefinitionExceptionSurrogate ConvertToSurrogate(in ConflictDefinitionException value)
        {
            return new ConflictDefinitionExceptionSurrogate()
            {
                InnerException = value.InnerException,
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                DefinitionType = (AbstractType)value.Data[nameof(ConflictDefinitionExceptionSurrogate.DefinitionType)]!,
                DefinitionSource = (string)value.Data[nameof(ConflictDefinitionExceptionSurrogate.DefinitionSource)]!,
                ConflictDefinition = (string)value.Data[nameof(ConflictDefinitionExceptionSurrogate.ConflictDefinition)]!
            };
        }
    }
}
