// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders
{
    using MARI.Framework.Builders.Models;

    using Microsoft.Extensions.Logging;

    using System.Threading.Tasks;

    /// <summary>
    /// Compiler to transform YAML description to mari <see cref="IDefinition"/>
    /// </summary>
    public interface IDefinitionCompiler
    {
        /// <summary>
        /// Compiles <see cref="IDefinition"/> from yaml files
        /// </summary>
        ValueTask<CompilationResult> CompileAsync(Stream content,
                                                  DefinitionParserSourceEnum sourceType = DefinitionParserSourceEnum.Yaml,
                                                  ILogger? logger = null,
                                                  CancellationToken token = default);

        /// <summary>
        /// Compiles <see cref="IDefinition"/> from yaml files
        /// </summary>
        ValueTask<CompilationResult> CompileAsync(string content,
                                                  DefinitionParserSourceEnum sourceType = DefinitionParserSourceEnum.Yaml,
                                                  ILogger? logger = null,
                                                  CancellationToken token = default);
    }
}
