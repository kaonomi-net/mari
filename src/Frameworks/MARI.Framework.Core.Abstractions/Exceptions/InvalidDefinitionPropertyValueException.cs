// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions.Resources;
    using MARI.Toolbox.Models;

    using System;

    /// <summary>
    /// Raised when definition's property is invalid.
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public sealed class InvalidDefinitionPropertyValueException : MARIBaseException<InvalidDefinitionPropertyValueException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDefinitionPropertyValueException"/> class.
        /// </summary>
        public InvalidDefinitionPropertyValueException(Type definitionType,
                                                       string propertyName,
                                                       string value,
                                                       string expectedDetails,
                                                       Exception? innerException = null)
            : this(MARIExceptionSR.InvalidDefinitionPropertyValueExceptionMessage.WithArguments(propertyName, definitionType, value, expectedDetails),
                   definitionType.GetAbstractType(),
                   propertyName,
                   value,
                   expectedDetails,
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Definition, MARIErrorCodes.PartType.Property, MARIErrorCodes.ErrorType.Invalid),
                   innerException)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidDefinitionPropertyValueException"/> class.
        /// </summary>
        internal InvalidDefinitionPropertyValueException(string message,
                                                         AbstractType definitionType,
                                                         string propertyName,
                                                         string value,
                                                         string expectedDetails,
                                                         ulong errorCode,
                                                         Exception? innerException) 
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(InvalidDefinitionPropertyValueExceptionSurrogate.DefinitionType), definitionType);
            this.Data.Add(nameof(InvalidDefinitionPropertyValueExceptionSurrogate.PropertyName), propertyName);
            this.Data.Add(nameof(InvalidDefinitionPropertyValueExceptionSurrogate.Value), value);
            this.Data.Add(nameof(InvalidDefinitionPropertyValueExceptionSurrogate.ExpectedDetails), expectedDetails);
        }
    }

    [GenerateSerializer]
    public struct InvalidDefinitionPropertyValueExceptionSurrogate : IMARIBaseExceptionSurrogate
    {
        [Id(0)]
        public string Message { get; set; }

        [Id(1)]
        public AbstractType DefinitionType { get; set; }

        [Id(2)]
        public string PropertyName { get; set; }

        [Id(3)]
        public string Value { get; set; }

        [Id(4)]
        public string ExpectedDetails { get; set; }

        [Id(5)]
        public ulong ErrorCode { get; set; }

        [Id(6)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter] 
    public sealed class InvalidDefinitionPropertyValueExceptionConverter : IConverter<InvalidDefinitionPropertyValueException, InvalidDefinitionPropertyValueExceptionSurrogate>
    {
        public InvalidDefinitionPropertyValueException ConvertFromSurrogate(in InvalidDefinitionPropertyValueExceptionSurrogate surrogate)
        {
            return new InvalidDefinitionPropertyValueException(surrogate.Message,
                                                               surrogate.DefinitionType,
                                                               surrogate.PropertyName,
                                                               surrogate.Value,
                                                               surrogate.ExpectedDetails,
                                                               surrogate.ErrorCode,
                                                               surrogate.InnerException);
        }

        public InvalidDefinitionPropertyValueExceptionSurrogate ConvertToSurrogate(in InvalidDefinitionPropertyValueException value)
        {
            return new InvalidDefinitionPropertyValueExceptionSurrogate()
            {
                Message = value.Message,
                InnerException = value.InnerException,
                ErrorCode = value.ErrorCode,
                DefinitionType = (AbstractType)value.Data[nameof(InvalidDefinitionPropertyValueExceptionSurrogate.DefinitionType)]!,
                PropertyName = (string)value.Data[nameof(InvalidDefinitionPropertyValueExceptionSurrogate.PropertyName)]!,
                Value = (string)value.Data[nameof(InvalidDefinitionPropertyValueExceptionSurrogate.Value)]!,
                ExpectedDetails = (string)value.Data[nameof(InvalidDefinitionPropertyValueExceptionSurrogate.ExpectedDetails)]!,
            };
        }
    }
}
