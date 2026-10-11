// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders
{
    using MARI.Toolbox.Abstractions.Services;

    /// <summary>
    /// Base interface of all the end part of builders
    /// </summary>
    public interface IDefinitionBaseBuilder<TDefinition>
    {
        /// <summary>
        /// Compile setup information to build <see cref="TDefinition"/>
        /// </summary>
        TDefinition Build();
    }

    /// <summary>
    /// Base interface of all the end part of builders
    /// </summary>
    public interface IDefinitionBaseBuilderByCompilation<TDefinition>
    {
        /// <summary>
        /// Compile setup information to compile <see cref="TDefinition"/>
        /// </summary>
        /// <param name="hashService"><see cref="HashSHA256Service"/> by default.</param>
        /// <param name="fileSystemHandler"><see cref="FileSystemHandler"/> by default.</param>
        ValueTask<TDefinition> CompileAsync(IHashService? hashService = null,
                                            IFileSystemHandler? fileSystemHandler = null,
                                            ITimeManager? timeManager = null,
                                            CancellationToken token = default);
    }
}
