// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Models.Surrogates
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Models.Surrogates;

    using Orleans;

    [RegisterConverter]
    public sealed class ExecutionContextSurrogateConverter : IConverter<ExecutionContext, ExecutionContextSurrogate>
    {
        /// <inheritdoc/>
        public ExecutionContext ConvertFromSurrogate(in ExecutionContextSurrogate surrogate)
        {
            var ctx = new ExecutionContext(surrogate.FlowUID,
                                           surrogate.CurrentExecutionId,
                                           surrogate.ParentExecutionId);

            if (surrogate.ContextDataContainers is not null && surrogate.ContextDataContainers.Any() && ctx is IExecutionContextInternal internalCtx)
                ctx.InjectAllDataContext(surrogate.ContextDataContainers);

            return ctx;
        }

        /// <inheritdoc/>
        public ExecutionContextSurrogate ConvertToSurrogate(in ExecutionContext value)
        {
            return new ExecutionContextSurrogate()
            {
                FlowUID = value.FlowUID,
                CurrentExecutionId = value.CurrentExecutionId,
                ParentExecutionId = value.ParentExecutionId,
                ContextDataContainers = value.GetAllDataContext()
            };
        }
    }
}