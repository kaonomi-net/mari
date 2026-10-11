// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Node.Blackboard.Abstractions.Models;
    using MARI.Toolbox.Abstractions.Patterns.Strategy;

    using System;

    /// <summary>
    /// Provider of <see cref="BlackboardTemplateDefinition"/> using all the <see cref="IBlackboardTemplateDefinitionProviderSource"/>
    /// </summary>
    /// <seealso cref="IProviderStrategy{BlackboardTemplateDefinition, Guid}" />
    /// <seealso cref="IInitService" />
    public interface IBlackboardTemplateDefinitionProvider : IProviderStrategy<BlackboardTemplateDefinition, Guid>
    {
    }
}
