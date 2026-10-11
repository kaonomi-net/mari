// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.Models
{
    using MARI.Framework.Core.Abstractions;

    using MARI.Toolbox.Abstractions.Loggers;

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class DefinitionCompileOption(string? StorageConfigurationName);

    [Immutable]
    [Serializable]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public record class CompilationResult(bool Success,
                                          IReadOnlyCollection<SimpleLog>? Logs,
                                          DefinitionCompileOption? CompileOption,
                                          IReadOnlyCollection<IDefinition> Definitions);
}
