// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Builders
{
    using MARI.Framework.Node.Blackboard.Builders.Templates;

    /// <summary>
    /// Entry point to build <see cref="BlackboardTemplateDefinition"/>
    /// </summary>
    public static class BlackboardTemplate
    {
        #region Methods

        /// <summary>
        /// Builds the specified name identifier.
        /// </summary>
        /// <param name="nameIdentifier">A Blackboard template could be identify by is <see cref="BlackboardTemplateDefinition.Uid"/> by also by his <see cref="BlackboardTemplateDefinition.NameIdentifier"/></param>
        public static IBlackboardTemplateBuilder Build(string nameIdentifier, Guid? fixUid = null)
        {
            return new BlackboardTemplateBuilder(nameIdentifier, fixUid);
        }

        #endregion
    }
}