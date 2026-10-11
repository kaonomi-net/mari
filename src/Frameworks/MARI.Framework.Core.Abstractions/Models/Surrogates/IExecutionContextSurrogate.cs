// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Models.Surrogates
{
    using System;

    public interface IExecutionContextSurrogate
    {
        /// <inheritdoc cref="ExecutionContext.FlowUID" />
        Guid FlowUID { get; set; }

        /// <inheritdoc cref="ExecutionContext.CurrentExecutionId" />
        Guid CurrentExecutionId { get; set; }

        /// <inheritdoc cref="ExecutionContext.ParentExecutionId" />
        Guid? ParentExecutionId { get; set; }
    }
}
