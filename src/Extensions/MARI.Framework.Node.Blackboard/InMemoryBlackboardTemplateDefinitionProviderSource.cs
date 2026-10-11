// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard
{
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions.Doors;
    using MARI.Framework.Node.Blackboard.Abstractions;
    using MARI.Framework.Node.Blackboard.Abstractions.Models;

    using MARI.Toolbox.Patterns.Strategy;

    /// <summary>
    /// In memory blackboard template definition provider
    /// </summary>
    /// <seealso cref="ProviderStrategyBaseSource{DoorDefinition, Guid}" />
    /// <seealso cref="IDoorDefinitionProviderSource" />
    internal sealed class InMemoryBlackboardTemplateDefinitionProviderSource : InMemoryBaseDefinitionProvider<BlackboardTemplateDefinition>, IBlackboardTemplateDefinitionProviderSource
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryBlackboardTemplateDefinitionProviderSource"/> class.
        /// </summary>
        public InMemoryBlackboardTemplateDefinitionProviderSource()
            : base(null!)
        {
        }

        #endregion
    }
}
