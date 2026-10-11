// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Configurations
{
    using MARI.Framework.Cluster.Abstractions.Configurations.Builders;
    using MARI.Framework.Configurations;
    using MARI.Framework.Node.Blackboard.Abstractions;
    using MARI.Framework.Node.Blackboard.Abstractions.Models;
    using MARI.Framework.Node.Blackboard.Validators;
    using MARI.Framework.Node.Configurations;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    using System;

    /// <summary>
    /// Blackboard definition builder
    /// </summary>
    /// <seealso cref="IMARINodeBlackboardsBuilder" />
    /// <seealso cref="IMARINodeBlackboardsLocalDefinitionBuilder" />
    internal sealed class MARINodeBlackboardsBuilder : IMARINodeBlackboardsBuilder,
                                                            IMARINodeBlackboardsLocalDefinitionBuilder
    {
        #region Fields

        private readonly InMemoryBlackboardTemplateDefinitionProviderSource _inMemoryBlackboardTemplateDefinitionProviderSource;
        private readonly IMARIExtensionBuilderTool _wizardTools;
        private readonly IServiceCollection _serviceCollection;
        private int _buildOnceCounter;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MARINodeBlackboardsBuilder"/> class.
        /// </summary>
        public MARINodeBlackboardsBuilder(IMARIExtensionBuilderTool wizardTools)
        {
            this._wizardTools = wizardTools;
            this._serviceCollection = wizardTools.GetServiceCollection();

            this._inMemoryBlackboardTemplateDefinitionProviderSource = new InMemoryBlackboardTemplateDefinitionProviderSource();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the blackboard services.
        /// </summary>
        internal void BuildServices()
        {
            if (Interlocked.Increment(ref _buildOnceCounter) > 1)
                return;

            this._serviceCollection.AddSingleton<IBlackboardTemplateDefinitionProviderSource>(this._inMemoryBlackboardTemplateDefinitionProviderSource);
            this._serviceCollection.TryAddSingleton<IBlackboardTemplateDefinitionProvider, BlackboardTemplateDefinitionProvider>();
            this._serviceCollection.TryAddSingleton<IBlackboardProvider, BlackboardProvider>();
            this._serviceCollection.TryAddSingleton<IBlackboardDataLogicalTypeRuleValidatorProvider, BlackboardDataLogicalTypeRuleValidatorProvider>();
        }

        /// <inheritdoc />
        public IMARINodeBlackboardsBuilder AddInMemoryDefinitionProvider(Action<IMARINodeBlackboardsLocalDefinitionBuilder> builder)
        {
            builder?.Invoke(this);
            return this;
        }

        /// <inheritdoc />
        public IMARINodeBlackboardsLocalDefinitionBuilder SetupTemplates(params BlackboardTemplateDefinition[] templates)
        {
            foreach (var tmpl in templates)
                this._inMemoryBlackboardTemplateDefinitionProviderSource.AddOrUpdate(tmpl);
            return this;
        }

        /// <inheritdoc />
        public IMARINodeBlackboardsBuilder UseInMemoryStorageForBoardState()
        {
            ((ISiloBuilder)this._wizardTools.SourceOrleanBuilder).AddMemoryGrainStorage(BlackboardConstants.BlackboardStateStorageConfigurationKey);
            return this;
        }

        /// <inheritdoc />
        public IMARINodeBlackboardsBuilder UseInMemoryStorageForRegistryState()
        {
            ((ISiloBuilder)this._wizardTools.SourceOrleanBuilder).AddMemoryGrainStorage(BlackboardConstants.BlackboardRegistryStorageConfigurationKey);
            return this;
        }

        /// <inheritdoc />
        public IMARINodeBlackboardsBuilder UseInMemoryStorageForRecords()
        {
            var memoryBuilder = this._wizardTools.TryGetBuilder<IMARINodeMemoryBuilder>();
            memoryBuilder.UseInMemoryRepository(BlackboardConstants.BlackboardStorageRecordsConfigurationKey);
            return this;
        }

        #endregion
    }
}
