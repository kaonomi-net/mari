// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Node.Blackboard.Abstractions.Models;
    using MARI.Toolbox.Abstractions.Patterns.Strategy;

    /// <summary>
    /// Source provider of <see cref="BlackboardTemplateDefinition"/>
    /// </summary>
    /// <seealso cref="IProviderStrategySource{BlackboardTemplateDefinition, Guid}" />
    /// <seealso cref="IInitService" />
    public interface IBlackboardTemplateDefinitionProviderSource : IDefinitionSourceProvider<BlackboardTemplateDefinition>
    {
    }
}
