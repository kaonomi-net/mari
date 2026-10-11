// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions.Resources;

    using System;

    /// <summary>
    /// Raised when sequence id provide is missing from the <see cref="IWorkfloDefintionManager"/>
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public sealed class SequenceDefinitionMissingException : MARIBaseException<SequenceDefinitionMissingException>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceDefinitionMissingException"/> class.
        /// </summary>
        public SequenceDefinitionMissingException(Guid worflowDefinitionId, Exception? innerException = null)
            : base(MARIExceptionSR.SequenceDefinitionMissing.WithArguments(worflowDefinitionId),
                   MARIErrorCodes.Build(MARIErrorCodes.Categories.Sequence, MARIErrorCodes.PartType.Definition, MARIErrorCodes.ErrorType.Missing),
                   innerException)
        {
            this.Data.Add(nameof(SequenceDefinitionMissingExceptionSurrogate.WorflowDefinitionId), worflowDefinitionId);
        }
    }

    [GenerateSerializer]
    public struct SequenceDefinitionMissingExceptionSurrogate
    {
        [Id(0)]
        public Guid WorflowDefinitionId { get; set; }

        [Id(1)]
        public Exception? InnerException { get; set; }
    }

    [RegisterConverter]
    public sealed class SequenceDefinitionMissingExceptionConverter : IConverter<SequenceDefinitionMissingException, SequenceDefinitionMissingExceptionSurrogate>
    {
        public SequenceDefinitionMissingException ConvertFromSurrogate(in SequenceDefinitionMissingExceptionSurrogate surrogate)
        {
            return new SequenceDefinitionMissingException(surrogate.WorflowDefinitionId, surrogate.InnerException);
        }

        public SequenceDefinitionMissingExceptionSurrogate ConvertToSurrogate(in SequenceDefinitionMissingException value)
        {
            return new SequenceDefinitionMissingExceptionSurrogate()
            {
                InnerException = value.InnerException,
                WorflowDefinitionId = (Guid)value.Data[nameof(SequenceDefinitionMissingExceptionSurrogate.WorflowDefinitionId)]!
            };
        }
    }
}
