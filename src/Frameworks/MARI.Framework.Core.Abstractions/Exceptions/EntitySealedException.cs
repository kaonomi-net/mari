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
    /// <seealso cref="MARIBaseException{EntitySealedException}" />
    public sealed class EntitySealedException : MARIBaseException<EntitySealedException>
    {
        #region Ctor

        public EntitySealedException(object? entity,
                                     string entityIdentity,
                                     Exception? innerException = null)

            : this(MARIExceptionSR.EntitySealedExceptionMessage.WithArguments(entity is ISupportDebugDisplayName debug ? debug.ToDebugDisplayName() : entity?.ToString(), entityIdentity),
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Entity, genericType: MARIErrorCodes.ErrorType.Sealed),
                   entity is not null ? (ConcretBaseType)entity.GetType().GetAbstractType() : null,
                   entityIdentity,
                   innerException)
        {
                
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySealedException"/> class.
        /// </summary>
        internal EntitySealedException(string message,
                                       ulong errorCode,
                                       ConcretBaseType? entityType,
                                       string? entityIdentity,
                                       Exception? innerException) 
            : base(message, 
                   errorCode, 
                   innerException)
        {
            this.Data[nameof(EntitySealedExceptionSurrogate.EntityType)] = entityType;
            this.Data[nameof(EntitySealedExceptionSurrogate.EntityIdentity)] = entityIdentity;
        }

        #endregion
    }

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record struct EntitySealedExceptionSurrogate(string Message,
                                                        ulong ErrorCode,
                                                        IConcretTypeSurrogate? EntityType,
                                                        string? EntityIdentity,
                                                        Exception? InnerException);

    [RegisterConverter]
    public sealed class EntitySealedExceptionConverter : IConverter<EntitySealedException, EntitySealedExceptionSurrogate>
    {
        /// <inheritdoc />
        public EntitySealedException ConvertFromSurrogate(in EntitySealedExceptionSurrogate surrogate)
        {
            return new EntitySealedException(surrogate.Message,
                                             surrogate.ErrorCode,
                                             surrogate.EntityType is not null ?  ConcretBaseTypeConverter.ConvertFromSurrogate(surrogate.EntityType) : null,
                                             surrogate.EntityIdentity,
                                             surrogate.InnerException);
        }

        /// <inheritdoc />
        public EntitySealedExceptionSurrogate ConvertToSurrogate(in EntitySealedException value)
        {
            return new EntitySealedExceptionSurrogate()
            {
                EntityType = ConcretBaseTypeConverter.ConvertToSurrogate((ConcretBaseType?)value.Data[nameof(EntitySealedExceptionSurrogate.EntityType)]!),
                EntityIdentity = (string?)value.Data[nameof(EntitySealedExceptionSurrogate.EntityIdentity)],
                Message = value.Message,
                ErrorCode = value.ErrorCode,
                InnerException = value.InnerException
            };
        }
    }
}
