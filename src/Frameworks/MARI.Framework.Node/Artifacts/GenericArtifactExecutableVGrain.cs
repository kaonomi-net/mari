// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Artifacts
{
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Artifacts;
    using MARI.Framework.Node.Abstractions.Artifacts;
    using MARI.Toolbox;

    using Microsoft.Extensions.Logging;

    using Orleans.Concurrency;

    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// VGrain in charge to using external code to solve the input. (Script, c++ binary, python, ...) 
    /// </summary>
    /// <seealso cref="VGrainBase{TVGrainInterface}" />
    public sealed class GenericArtifactExecutableVGrain : ArtifactExecutableBaseVGrain<IGenericArtifactExecutableVGrain>, IGenericArtifactExecutableVGrain
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericArtifactExecutableVGrain"/> class.
        /// </summary>
        public GenericArtifactExecutableVGrain(ILogger<IGenericArtifactExecutableVGrain> logger,
                                               IArtifactDefinitionProvider artifactProvider,
                                               IArtifactExecutorFactory artifactExecutorFactory,
                                               ILoggerFactory loggerFactory)
            : base(logger, artifactProvider, artifactExecutorFactory, loggerFactory)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public void Fire(IExecutionContext<Guid> executionContext)
        {
            LogOnFault(base.RunAsync(executionContext));
        }

        Task IGenericContextedExecutor<Guid>.Fire(IExecutionContext<Guid> executionContext)
        {
            // [OneWay] on the interface already prevents the caller from waiting
            return LogOnFault(base.RunAsync(executionContext));
        }

        /// <inheritdoc />
        public Task Fire<TInput>(TInput? input, IExecutionContext<Guid> executionContext)
        {
            return LogOnFault(base.RunAsync<NoneType, TInput>(input, executionContext));
        }

        /// <inheritdoc />
        public Task<TOutput?> RunAsync<TOutput>(IExecutionContext<Guid> executionContext)
        {
            return base.RunAsync<TOutput, NoneType>(NoneType.Instance, executionContext);
        }

        /// <inheritdoc />
        public Task RunAsync(IExecutionContext<Guid> executionContext)
        {
            return base.RunAsync<NoneType>(executionContext);
        }

        /// <inheritdoc />
        public Task<TOutput?> RunAsync<TOutput, TInput>(TInput? input, IExecutionContext<Guid> executionContext)
        {
            return base.RunAsync<TOutput, TInput>(input, executionContext);
        }

        /// <inheritdoc />
        public Task RunWithInputAsync<TInput>(TInput? input, IExecutionContext<Guid> executionContext)
        {
            return RunAsync<NoneType, TInput>(input, executionContext);
        }

        /// <inheritdoc />
        [ReadOnly]
        public Task<TOutput?> ConcurrentRunAsync<TOutput>(IExecutionContext<Guid> executionContext)
        {
            return RunAsync<TOutput>(executionContext);
        }

        /// <inheritdoc />
        [ReadOnly]
        public Task<TOutput?> ConcurrentRunAsync<TOutput, TInput>(TInput? input, IExecutionContext<Guid> executionContext)
        {
            return RunAsync<TOutput, TInput>(input, executionContext);
        }

        /// <inheritdoc />
        [ReadOnly]
        public Task ConcurrentRunAsync(IExecutionContext<Guid> executionContext)
        {
            return RunAsync(executionContext);
        }

        /// <inheritdoc />
        [ReadOnly]
        public Task ConcurrentRunWithInputAsync<TInput>(TInput? input, IExecutionContext<Guid> executionContext)
        {
            return RunWithInputAsync<TInput>(input, executionContext);
        }

        /// <inheritdoc />
        protected override Guid GetArtifactId<TInput>(TInput? input, IExecutionContext executionContext) 
            where TInput : default
        {
            var fullCtx = executionContext as IExecutionContext<Guid>;
            Debug.Assert(fullCtx != null);
            return fullCtx.Configuration;
        }

        #endregion
    }
}
