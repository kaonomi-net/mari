// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// KEEP : MARI.Framework.Configurations
namespace MARI.Framework.Configurations
{
    using MARI.Framework.Builders;

    using Microsoft.Extensions.DependencyInjection;

    public static class MARIBuilderExtensions
    {
        /// <summary>
        /// Adds the definition compiler.
        /// </summary>
        public static TWizard AddDefinitionCompiler<TWizard>(this TWizard builder)
            where TWizard : IBuilderMARIBaseWizard
        {
            builder.GetServiceCollection().AddSingleton<IDefinitionCompiler, DefinitionCompiler>();
            return builder;
        }
    }
}
