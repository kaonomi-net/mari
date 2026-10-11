// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Artifacts
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Artifacts;
    using MARI.Framework.Node.Abstractions.Artifacts;
    using MARI.Framework.Node.Abstractions.Exceptions;

    using MARI.Toolbox.Abstractions.Services;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="MARI.Framework.Node.Abstractions.Artifacts.IArtifactExecutorDedicatedFactory" />
    public abstract class ArtifactExecutorBaseDedicatedFactory : IArtifactExecutorDedicatedFactory
    {
        #region Fields

        private readonly IFileSystemHandler _fileSystemHandler;
        
        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactExecutorBaseDedicatedFactory"/> class.
        /// </summary>
        protected ArtifactExecutorBaseDedicatedFactory(IFileSystemHandler fileSystemHandler)
        {
            this._fileSystemHandler = fileSystemHandler;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public abstract ValueTask<IArtifactExternalCodeExecutor> BuildNewExecutorAsync(ArtifactExecutableDefinition artifactExecutableDefinition,
                                                                                       IArtifactExternalCodeExecutor? previousExecutor,
                                                                                       IExecutionContext executionContext,
                                                                                       ILogger logger,
                                                                                       CancellationToken token);

        /// <inheritdoc />
        public abstract bool CanManaged(ArtifactExecutableDefinition artifactExecutableDefinition);

        /// <inheritdoc />
        public abstract ValueTask<bool> CheckExecutorValidityAsync(ArtifactExecutableDefinition artifactExecutableDefinition,
                                                                   IArtifactExternalCodeExecutor artifact,
                                                                   IExecutionContext executionContext,
                                                                   ILogger logger,
                                                                   CancellationToken token);

        #region Tools

        /// <summary>
        /// Gets the localIsFine folder path associate to <see cref="ArtifactExecutableDefinition"/>
        /// </summary>
        protected async ValueTask<Uri> GetLocalFolderPathAsync(ArtifactExecutableDefinition artifactCodePackageResource,
                                                               CancellationToken token)
        {
            var tmpRootPath = await this._fileSystemHandler.GetTemporaryFolderAsync(false, token);
            var artifactFolder = nameof(Artifacts);
            var version = artifactCodePackageResource.Version?.ToString() ?? "Unversionned";

            return new Uri(Path.Combine(tmpRootPath,
                                        artifactFolder,
                                        SanitizeFolderName(artifactCodePackageResource.DisplayName) + " - " + artifactCodePackageResource.Uid,
                                        SanitizeFolderName(version)) + "/");
        }

        /// <summary>
        /// Clean up the target artifact install folder
        /// </summary>
        protected async ValueTask CleanUpLocalFolderAsync(ArtifactExecutableDefinition definition, CancellationToken token)
        {
            var rootInstallPath = await GetLocalFolderPathAsync(definition, token);

            if (rootInstallPath is not null)
                await this._fileSystemHandler.DeleteFolder(rootInstallPath, true);
        }

        /// <summary>
        /// Installs in local the artifact data
        /// </summary>
        protected ValueTask InstallInLocalAsync(ArtifactExecutableDefinition artifactCodePackageResource,
                                                Uri root,
                                                IReadOnlyCollection<Uri> targetFiles,
                                                ILogger logger,
                                                IExecutionContext? executionContext,
                                                CancellationToken token)
        {
            if (artifactCodePackageResource.PackageType == ArtifactPackageTypeEnum.Directory)
            {
                return InstallInLocalAsync(artifactCodePackageResource,
                                           artifactCodePackageResource.PackageSource.OriginalString,
                                           root,
                                           executionContext,
                                           token);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Installs in local the artifact data from temp local folder
        /// </summary>
        protected async ValueTask InstallInLocalAsync(ArtifactExecutableDefinition artifactCodePackageResource,
                                                    string tmpLocalResourceRootFolder,
                                                    Uri root,
                                                    IExecutionContext? executionContext,
                                                    CancellationToken token)
        {
            var sourceUri = this._fileSystemHandler.MakeUriAbsolute(tmpLocalResourceRootFolder);

            // Folder against which the relative package files are resolved
            var sourceRoot = new Uri(sourceUri, "./");
            var targetRoot = new Uri(root, "./");

            foreach (var sourceFile in artifactCodePackageResource.PackageFiles)
            {
                var source = new Uri(sourceUri, sourceFile);
                var target = new Uri(root, sourceFile);

                if (!IsUnderRoot(sourceRoot, source) || !IsUnderRoot(targetRoot, target))
                    throw new ArtifactPreparationFailedException("Package file '{0}' resolves outside the artifact folder".WithArguments(sourceFile), executionContext);

                if (!await this._fileSystemHandler.CopyFromAsync(source, target, true))
                    throw new ArtifactPreparationFailedException("Could not copy '{0}' from '{1}' to '{2}'".WithArguments(sourceFile, source, target), executionContext);
            }
        }

        /// <summary>
        /// Check that <paramref name="candidate"/> is located under <paramref name="root"/> (prevent path traversal)
        /// </summary>
        private static bool IsUnderRoot(Uri root, Uri candidate)
        {
            if (root.IsFile && candidate.IsFile)
            {
                var rootPath = Path.GetFullPath(root.LocalPath);
                if (!Path.EndsInDirectorySeparator(rootPath))
                    rootPath += Path.DirectorySeparatorChar;

                var candidatePath = Path.GetFullPath(candidate.LocalPath);
                return candidatePath.StartsWith(rootPath, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            }

            return string.Equals(root.Scheme, candidate.Scheme, StringComparison.OrdinalIgnoreCase) &&
                   candidate.AbsoluteUri.StartsWith(root.AbsoluteUri, StringComparison.Ordinal);
        }

        /// <summary>
        /// Remove from <paramref name="name"/> any characters not allowed in a folder name (including directory separators)
        /// </summary>
        private static string SanitizeFolderName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            var invalidChars = Path.GetInvalidFileNameChars();
            var chars = name.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '/' || chars[i] == '\\' || Array.IndexOf(invalidChars, chars[i]) >= 0)
                    chars[i] = '_';
            }

            var result = new string(chars).Trim();

            // Avoid relative segments like '.' or '..'
            if (result.Trim('.').Length == 0)
                return string.Empty;

            return result;
        }

        #endregion

        #endregion
    }
}
