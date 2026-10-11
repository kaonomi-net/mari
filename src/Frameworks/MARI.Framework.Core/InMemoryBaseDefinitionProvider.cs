// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core
{
    using MARI.Framework.Core.Abstractions;

    using MARI.Toolbox.Patterns.Strategy;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Base InMemory defintion provider
    /// </summary>
    public abstract class InMemoryBaseDefinitionProvider<TDefinition> : ProviderStrategyBaseSource<TDefinition, Guid>, IDefinitionInMemoryFillSourceProvider
        where TDefinition : class, IDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryBaseDefinitionProvider{TDefinition}"/> class.
        /// </summary>
        protected InMemoryBaseDefinitionProvider(IServiceProvider serviceProvider, IEnumerable<(Guid key, TDefinition value)>? initValues = null) 
            : base(serviceProvider, initValues)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds or update a new artefact
        /// </summary>
        public void AddOrUpdate(TDefinition defintion)
        {
            base.SafeAddOrReplace(defintion.Uid, defintion);
        }

        /// <inheritdoc />
        public bool CanStore(IDefinition definition)
        {
            return definition is TDefinition;
        }

        /// <inheritdoc />
        public bool TryStore(IDefinition definition)
        {
            if (definition is TDefinition tr)
            {
                AddOrUpdate(tr);
                return true;
            }

            return false;
        }

        #endregion
    }
}
