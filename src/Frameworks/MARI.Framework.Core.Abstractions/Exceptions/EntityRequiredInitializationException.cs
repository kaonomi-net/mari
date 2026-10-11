// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions.Resources;
    using MARI.Framework.Core.Abstractions.Surrogates;

    using MARI.Toolbox.Abstractions.Supports;
    using MARI.Toolbox.Models;

    using System;
    using System.ComponentModel;

    /// <summary>
    /// Raised when we try to modify a sealed entity
    /// </summary>
    /// <seealso cref="MARIBaseException{EntityRequiredInitializationException}" />
    public sealed class EntityRequiredInitializationException : MARIBaseException<EntityRequiredInitializationException>
    {
        #region Ctor

        public EntityRequiredInitializationException(object? entity,
                                     string entityIdentity,
                                     Exception? innerException = null)

            : this(MARIExceptionSR.EntityRequiredInitializationExceptionMessage.WithArguments(entity is ISupportDebugDisplayName debug ? debug.ToDebugDisplayName() : entity?.ToString(), entityIdentity),
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Entity, genericType: MARIErrorCodes.ErrorType.NotInitialized),
                   entity is not null ? (ConcretBaseType)entity.GetType().GetAbstractType() : null,
                   entityIdentity,
                   innerException)
        {
                
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRequiredInitializationException"/> class.
        /// </summary>
        internal EntityRequiredInitializationException(string message,
                                       ulong errorCode,
                                       ConcretBaseType? entityType,
                                       string? entityIdentity,
                                       Exception? innerException) 
            : base(message, 
                   errorCode, 
                   innerException)
        {
            this.Data[nameof(EntityRequiredInitializationExceptionSurrogate.EntityType)] = entityType;
            this.Data[nameof(EntityRequiredInitializationExceptionSurrogate.EntityIdentity)] = entityIdentity;
        }

        #endregion
    }

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct EntityRequiredInitializationExceptionSurrogate(string Message,
                                                                        ulong ErrorCode,
                                                                        IConcretTypeSurrogate? EntityType,
                                                                        string? EntityIdentity,
                                                                        Exception? InnerException);

    [RegisterConverter]
    public sealed class EntityRequiredInitializationExceptionConverter : IConverter<EntityRequiredInitializationException, EntityRequiredInitializationExceptionSurrogate>
    {
        /// <inheritdoc />
        public EntityRequiredInitializationException ConvertFromSurrogate(in EntityRequiredInitializationExceptionSurrogate surrogate)
        {
            return new EntityRequiredInitializationException(surrogate.Message,
                                                             surrogate.ErrorCode,
                                                             surrogate.EntityType is not null ? ConcretBaseTypeConverter.ConvertFromSurrogate(surrogate.EntityType) : null,
                                                             surrogate.EntityIdentity,
                                                             surrogate.InnerException);
        }

        /// <inheritdoc />
        public EntityRequiredInitializationExceptionSurrogate ConvertToSurrogate(in EntityRequiredInitializationException value)
        {
            return new EntityRequiredInitializationExceptionSurrogate()
            {
                EntityType = ConcretBaseTypeConverter.ConvertToSurrogate((ConcretBaseType?)value.Data[nameof(EntityRequiredInitializationExceptionSurrogate.EntityType)]!),
                EntityIdentity = (string?)value.Data[nameof(EntityRequiredInitializationExceptionSurrogate.EntityIdentity)],
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                InnerException = value.InnerException
            };
        }
    }
}
