// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Customizations;
    using MARI.Framework.Core.Abstractions.Deferred;
    using MARI.Framework.Core.Abstractions.Models.References;
    using MARI.Framework.Core.Abstractions.Repositories;
    using MARI.Framework.Core.Executions;

    using MARI.Toolbox;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using Orleans.Runtime;

    using System;

    /// <summary>
    /// Default implementation of <see cref="IMARIExecutionHandler"/>
    /// </summary>
    /// <seealso cref="IMARIExecutionHandler" />
    internal sealed class MARIExecutionHandler : IMARIExecutionHandler
    {
        #region Fields

        private readonly IDeferredAwaiterHandler _deferredAwaiterHandler;
        private readonly ILogger<IMARIExecutionHandler> _logger;
        private readonly IMARISerializer _mariSerializer;
        private readonly IVGrainProvider _vgrainProvider;
        private readonly IGrainFactory _grainFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MARIExecutionHandler"/> class.
        /// </summary>
        public MARIExecutionHandler(IVGrainProvider vgrainProvider,
                                         IGrainFactory grainFactory,
                                         IMARISerializer mariSerializer,
                                         IDeferredAwaiterHandler deferredAwaiterHandler,
                                         ILogger<IMARIExecutionHandler>? logger = null)
        {
            this._vgrainProvider = vgrainProvider;
            this._mariSerializer = mariSerializer;
            this._deferredAwaiterHandler = deferredAwaiterHandler;
            this._grainFactory = grainFactory;
            this._logger = logger ?? NullLogger<IMARIExecutionHandler>.Instance;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IExecutionDirectBuilder<TVGrain> VGrain<TVGrain>()
            where TVGrain : IVGrain
        {
            return VGrainImpl<TVGrain>(null);
        }

        /// <inheritdoc />
        public IExecutionDirectBuilder<TVGrain> VGrain<TVGrain>(Guid id, string? customIdPart = null)
            where TVGrain : IVGrain
        {
            return VGrainImpl<TVGrain>(GrainIdKeyExtensions.CreateGuidKey(id, customIdPart));
        }

        /// <inheritdoc />
        public IExecutionDirectBuilder<TVGrain> VGrain<TVGrain>(long id, string? customIdPart = null) 
            where TVGrain : IVGrain
        {
            return VGrainImpl<TVGrain>(GrainIdKeyExtensions.CreateIntegerKey(id, customIdPart));
        }

        /// <inheritdoc />
        public IExecutionDirectBuilder<TVGrain> VGrain<TVGrain>(string id) where TVGrain : IVGrain
        {
            return VGrainImpl<TVGrain>(IdSpan.Create(id));
        }

        /// <inheritdoc />
        public IExecutionRefBuilder VGrain(Uri refId)
        {
            if (RefIdHelper.IsRefId(refId) == false)
                throw new InvalidDataException("'{0}' is not a valid mari ref id");

            RefIdHelper.Explode(refId, out var type, out var @namespace, out var sni);

            if (type != Abstractions.Enums.RefTypeEnum.VGrain &&
                type != Abstractions.Enums.RefTypeEnum.VGrainImplementation)
            {
                throw new InvalidDataException("'{0}' is not a valid mari ref id only VGrain and VGrainImplementation are allowed");
            }

            return VGrain(new RefVGrainQuery(sni, @namespace));
        }

        /// <inheritdoc />
        public IExecutionRefBuilder VGrain(RefVGrainQuery refIdQuery, string id)
        {
            return VGrainImpl(refIdQuery, IdSpan.Create(id));
        }

        /// <inheritdoc />
        public IExecutionRefBuilder VGrain(RefVGrainQuery refIdQuery, Guid id, string? customIdPart = null)
        {
            return VGrainImpl(refIdQuery, GrainIdKeyExtensions.CreateGuidKey(id, customIdPart));
        }

        /// <inheritdoc />
        public IExecutionRefBuilder VGrain(RefVGrainQuery refIdQuery, long id, string? customIdPart = null)
        {
            return VGrainImpl(refIdQuery, GrainIdKeyExtensions.CreateIntegerKey(id, customIdPart));
        }

        /// <inheritdoc />
        public IExecutionRefBuilder VGrain(RefVGrainQuery refIdQuery)
        {
            return VGrainImpl(refIdQuery, null);
        }

        /// <inheritdoc />
        public IExecutionFlowLauncher Sequence(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null)
        {
            return SequenceImpl<NoneType>(sequenceId, cfgBuilder, null);
        }

        /// <inheritdoc />
        public IExecutionFlowLauncher Sequence(Guid sequenceId, in ExecutionCustomizationDescriptions? customizationDescriptions)
        {
            return SequenceImpl<NoneType>(sequenceId, null, customizationDescriptions);
        }

        /// <inheritdoc />
        public IExecutionBuilder<TInput, IExecutionFlowLauncher> Sequence<TInput>(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null)
        {
            return SequenceImpl<TInput>(sequenceId, cfgBuilder, null);
        }

        /// <inheritdoc />
        public IExecutionBuilder<TInput, IExecutionFlowLauncher> Sequence<TInput>(Guid sequenceId, in ExecutionCustomizationDescriptions? customizationDescriptions)
        {
            return SequenceImpl<TInput>(sequenceId, null, customizationDescriptions);
        }

        /// <inheritdoc />
        public IExecutionBuilder<object, IExecutionFlowLauncher> SequenceWithInput(Guid sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null)
        {
            return SequenceImpl<object>(sequenceId, cfgBuilder, null);
        }

        /// <inheritdoc />
        public IExecutionBuilder<object, IExecutionFlowLauncher> SequenceWithInput(Guid sequenceId, in ExecutionCustomizationDescriptions? customizationDescriptions)
        {
            return SequenceImpl<object>(sequenceId, null, customizationDescriptions);
        }

        /// <inheritdoc />
        public IExecutionFlowLauncher Sequence(RefSequenceQuery sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IExecutionFlowLauncher Sequence(RefSequenceQuery sequenceId, in ExecutionCustomizationDescriptions? customizationDescriptions)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IExecutionBuilder<TInput, IExecutionFlowLauncher> Sequence<TInput>(RefSequenceQuery sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IExecutionBuilder<TInput, IExecutionFlowLauncher> Sequence<TInput>(RefSequenceQuery sequenceId, in ExecutionCustomizationDescriptions? customizationDescriptions)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IExecutionBuilder<object, IExecutionFlowLauncher> SequenceWithInput(RefSequenceQuery sequenceId, Action<IExecutionConfigurationBuilder>? cfgBuilder = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IExecutionBuilder<object, IExecutionFlowLauncher> SequenceWithInput(RefSequenceQuery sequenceId, in ExecutionCustomizationDescriptions? customizationDescriptions)
        {
            throw new NotImplementedException();
        }

        #region Tools

        private IExecutionRefBuilder VGrainImpl(RefVGrainQuery refIdQuery, IdSpan? forcedGrainId)
        {
            return new ExecutionRefBuilder(this._grainFactory, refIdQuery, forcedGrainId);
        }

        /// <inheritdoc />
        private IExecutionDirectBuilder<TVGrain> VGrainImpl<TVGrain>(IdSpan? forcedGrainId)
            where TVGrain : IVGrain
        {
            return new ExecutionDirectBuilder<TVGrain>(this._vgrainProvider, this._logger, forcedGrainId);
        }

        /// <inheritdoc />
        private ExecutionFlowLauncher<ISequenceExecutorVGrain, TInput> SequenceImpl<TInput>(Guid sequenceId,
                                                                                              Action<IExecutionConfigurationBuilder>? cfgBuilder,
                                                                                              in ExecutionCustomizationDescriptions? customizationDescriptions)
        {
            var executionCustomization = customizationDescriptions;

            if (cfgBuilder != null)
            {
                var builder = new ExecutionConfigurationBuilder();
                cfgBuilder.Invoke(builder);

                // To config definition
                executionCustomization = builder.Build();
            }

            return new ExecutionFlowLauncher<ISequenceExecutorVGrain, TInput>(sequenceId,
                                                                              this._vgrainProvider,
                                                                              this._logger,
                                                                              this._mariSerializer,
                                                                              executionCustomization,
                                                                              this._deferredAwaiterHandler);
        }

        #endregion

        #endregion
    }
}
