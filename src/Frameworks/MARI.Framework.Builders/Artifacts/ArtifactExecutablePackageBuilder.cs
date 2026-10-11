// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.Artifacts
{
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Artifacts;
    using MARI.Framework.Core.Abstractions.Configurations;
    using MARI.Framework.Core.Abstractions.Enums;

    using MARI.Toolbox.Abstractions.Services;
    using MARI.Toolbox.Services;

    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Builder used to setup information <see cref="ArtifactCodePackageResource"/>
    /// </summary>
    /// <seealso cref="IArtifactExecutablePackageResourceBuilder" />
    internal sealed class ArtifactExecutablePackageBuilder : IArtifactExecutablePackageResourceBuilder,
                                                             IArtifactCodePackageResourceBuilderFrom,
                                                             IArtifactCodePackageResourceBuilderFromSource,
                                                             IArtifactCodePackageResourceEnvironmentBuilder,
                                                             IArtifactCodePackageResourceBuilderFinalizer
    {
        #region Fields

        private readonly Dictionary<string, ConfigurationBaseDefinition> _configs;
        private readonly HashSet<string> _packageFiles;

        private readonly HashSet<string> _packageRecurciveTemplateFiles;
        private readonly HashSet<string> _packageTemplateFiles;
        
        private readonly HashSet<string> _packageExcludeFiles;
        private readonly HashSet<string> _packageExcludeDir;
        private readonly HashSet<Regex> _packageExcludeReg;
        private readonly string _simpleNameIdentifier;
        private readonly string _displayName;
        private readonly Guid _uid;

        private IDefinitionBaseBuilder<ArtifactExecutableEnvironmentDefinition>? _environmentBuilder;
        private IReadOnlyCollection<string>? _arguments;
        private ArtifactPackageTypeEnum? _packageType;
        private Version? _executorVersion;
        private string? _executablePath;
        private bool _allowPersistence;
        private string? _executor;
        private char? _executableArgSeparator;
        private string[]? _executorArgs;
        private Uri? _packageSource;
        private Version? _packageVersion;
        private ArtifactExecVerboseEnum _verbose;
        private DefinitionMetaData? _definitionMetaData;

        #endregion

        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactExecutablePackageBuilder"/> class.
        /// </summary>
        internal ArtifactExecutablePackageBuilder(string simpleNameIdentifier, string? displayName = null, Guid? uid = null)
        {
            this._configs = new Dictionary<string, ConfigurationBaseDefinition>();

            this._packageFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this._packageRecurciveTemplateFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this._packageTemplateFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            this._packageExcludeDir = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this._packageExcludeFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            this._packageExcludeReg = new HashSet<Regex>();

            this._simpleNameIdentifier = simpleNameIdentifier;
            this._displayName = displayName ?? simpleNameIdentifier;
            this._uid = uid ?? Guid.NewGuid();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFinalizer Arguments(params string[] args)
        {
            this._arguments = args?.ToArray() ?? EnumerableHelper<string>.ReadOnlyArray;
            return this;
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFrom ExecuteBy(string executor, Version? version = null, char argumentSeparator = ':', params string[] executorArgs)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(executor);

            this._executor = executor;
            this._executableArgSeparator = argumentSeparator;
            this._executorArgs = executorArgs;
            this._executorVersion = version;
            return this;

        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFinalizer ExecuteFile(string exec)
        {
            ArgumentNullException.ThrowIfNull(exec);
            this._executablePath = exec;

            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFromSource IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFromSource>.AppendFiles(params string[] files)
        {
            AppendFilesImpl(files);
            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFromSource IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFromSource>.AppendFiles(string fileTemplate, bool recursive)
        {
            AppendFilesTemplateImpl(fileTemplate, recursive);
            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFinalizer IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFinalizer>.AppendFiles(params string[] files)
        {
            AppendFilesImpl(files);
            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFinalizer IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFinalizer>.AppendFiles(string fileTemplate, bool recursive)
        {
            AppendFilesTemplateImpl(fileTemplate, recursive);
            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFromSource IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFromSource>.ExcludePathRegex(Regex regex)
        {
            ExcludePathRegexImpl(regex);
            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFromSource IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFromSource>.ExcludeFiles(params string[] files)
        {
            ExcludeFilesImpl(files);
            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFromSource IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFromSource>.ExcludeDirectories(params string[] directories)
        {
            ExcludeDirectoriesImpl(directories);
            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFinalizer IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFinalizer>.ExcludePathRegex(Regex regex)
        {
            ExcludePathRegexImpl(regex);
            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFinalizer IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFinalizer>.ExcludeFiles(params string[] files)
        {
            ExcludeFilesImpl(files);
            return this;
        }

        /// <inheritdoc />
        IArtifactCodePackageResourceBuilderFinalizer IArtifactCodePackageResourceBuilderFiles<IArtifactCodePackageResourceBuilderFinalizer>.ExcludeDirectories(params string[] directories)
        {
            ExcludeDirectoriesImpl(directories);
            return this;
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFromSource From(Uri packageUri, ArtifactPackageTypeEnum packageTypeEnum, Version? version)
        {
            ArgumentNullException.ThrowIfNull(packageUri);

            this._packageVersion = version;
            this._packageSource = packageUri;
            this._packageType = packageTypeEnum;

            return this;
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFromSource From(string packageUri, ArtifactPackageTypeEnum packageTypeEnum, Version? version)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(packageUri);

            packageUri = packageUri.Trim();

            // For the system directory end with an '/' and file not. Uri rule
            if (packageTypeEnum == ArtifactPackageTypeEnum.Directory && !(packageUri.EndsWith("/") || packageUri.EndsWith("\\")))
            {
                packageUri += "/";
            }

            return From(new Uri(packageUri, UriKind.RelativeOrAbsolute), packageTypeEnum, version);
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFromSource From(Uri packageUri, ArtifactPackageTypeEnum packageTypeEnum, string? version = null)
        {
            return From(packageUri, packageTypeEnum, string.IsNullOrEmpty(version) ? null : Version.Parse(version));
        }
        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFromSource From(string packageUri, ArtifactPackageTypeEnum packageTypeEnum, string? version = null)
        {
            return From(packageUri, packageTypeEnum, string.IsNullOrEmpty(version) ? null : Version.Parse(version));
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFinalizer Persistent()
        {
            this._allowPersistence = true;
            return this;
        }

        /// <summary>
        /// Builds <see cref="ArtifactCodePackageResource"/>.
        /// </summary>
        public async ValueTask<ArtifactExecutableDefinition> CompileAsync(IHashService? hashService = null,
                                                                          IFileSystemHandler? fileSystemHandler = null,
                                                                          ITimeManager? timeManager = null,
                                                                          CancellationToken token = default)
        {
            fileSystemHandler ??= FileSystemHandler.Instance;
            hashService ??= HashSHA256Service.Instance;
            timeManager ??= TimeManager.Instance;

            ArgumentNullException.ThrowIfNull(this._packageSource);
            ArgumentNullException.ThrowIfNull(this._packageType);
            ArgumentNullException.ThrowIfNull(this._executablePath);

            var absolutePackageSource = fileSystemHandler.MakeUriAbsolute(this._packageSource);

            // Work on a local copy to keep the builder state untouched (CompileAsync may be called several times)
            var packageFiles = new HashSet<string>(this._packageFiles, StringComparer.OrdinalIgnoreCase);

            ExtractTemplateFiles(absolutePackageSource, fileSystemHandler, this._packageRecurciveTemplateFiles, true, packageFiles);
            ExtractTemplateFiles(absolutePackageSource, fileSystemHandler, this._packageTemplateFiles, false, packageFiles);

            foreach (var directory in this._packageExcludeDir)
            {
                var dirPrefix = NormalizeRelativePath(directory).TrimEnd('/') + "/";
                packageFiles.RemoveWhere(p => NormalizeRelativePath(p).StartsWith(dirPrefix, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var file in this._packageExcludeFiles)
            {
                var normalizedFile = NormalizeRelativePath(file);

                if (string.IsNullOrEmpty(normalizedFile))
                    continue;

                // An exclusion ending with a separator targets a directory : remove all files under it
                if (normalizedFile.EndsWith('/'))
                {
                    packageFiles.RemoveWhere(p => NormalizeRelativePath(p).StartsWith(normalizedFile, StringComparison.OrdinalIgnoreCase));
                    continue;
                }

                // Otherwise exact relative path equality
                packageFiles.RemoveWhere(p => string.Equals(NormalizeRelativePath(p), normalizedFile, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var reg in this._packageExcludeReg)
            {
                packageFiles.RemoveWhere(p => reg.IsMatch(p));
            }

            packageFiles.Add(this._executablePath);

            // Relative paths are resolved against the package source; the resolved (normalized) uri must stay under this root
            var packageRoot = new Uri(absolutePackageSource, ".");
            var packageFileUris = new List<Uri>(packageFiles.Count);
            foreach (var file in packageFiles)
            {
                var fileUri = new Uri(absolutePackageSource, file);
                if (!packageRoot.IsBaseOf(fileUri))
                    throw new InvalidOperationException("Package file '" + file + "' is outside the package root '" + packageRoot + "'");

                packageFileUris.Add(fileUri);
            }

            var hash = await hashService.GetHash(packageFileUris,
                                                 fileSystemHandler,
                                                 token);

            return new ArtifactExecutableDefinition(this._uid,
                                                    RefIdHelper.Generate(RefTypeEnum.Artifact, this._simpleNameIdentifier, this._definitionMetaData?.NamespaceIdentifier),
                                                    this._displayName,
                                                    this._packageVersion,
                                                    hash,
                                                    timeManager.UtcNow,
                                                    this._executablePath,
                                                    this._allowPersistence,
                                                    this._arguments ?? EnumerableHelper<string>.ReadOnlyArray,
                                                    (string.IsNullOrWhiteSpace(this._executor)
                                                          ? string.Empty
                                                          : (this._executorVersion is not null) 
                                                                    ? this._executor + ":" + this._executorVersion
                                                                    : this._executor),
                                                    this._executableArgSeparator,
                                                    this._executorArgs,
                                                    this._packageSource,
                                                    packageFiles.OrderBy(p => p).ToArray(),
                                                    this._packageType.GetValueOrDefault(),
                                                    this._environmentBuilder?.Build(),
                                                    this._definitionMetaData,
                                                    this._verbose,
                                                    this._configs?.Values);
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFinalizer ExecEnvironment(Action<IArtifactCodePackageResourceEnvironmentBuilder> envBuilder)
        {
            ArgumentNullException.ThrowIfNull(envBuilder);
            envBuilder(this);
            return this;
        }

        /// <inheritdoc />
        public void Builder(IDefinitionBaseBuilder<ArtifactExecutableEnvironmentDefinition> environmentBuilder)
        {
            this._environmentBuilder = environmentBuilder;
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFinalizer Verbose(ArtifactExecVerboseEnum verbose)
        {
            this._verbose = verbose;
            return this;
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFinalizer AddConfiguration<TConfigType>(string configName,
                                                                                          TConfigType configType,
                                                                                          bool secureDataTansfer = false)
        {
            this._configs[configName] = new ConfigurationDirectDefinition<TConfigType>(Guid.NewGuid(),
                                                                                       configName,
                                                                                       configName,
                                                                                       configType,
                                                                                       secureDataTansfer,
                                                                                       null);
            return this;
        }

        /// <inheritdoc />
        public IArtifactCodePackageResourceBuilderFinalizer RequiredConfiguration<TConfigType>(string configName,
                                                                                               string configSectionKey,
                                                                                               TConfigType? defaultValue = default,
                                                                                               bool secureDataTansfer = false)
        {
            this._configs[configName] = new ConfigurationFromSectionPathDefinition<TConfigType>(Guid.NewGuid(),
                                                                                                configName,
                                                                                                configName,
                                                                                                configSectionKey,
                                                                                                defaultValue,
                                                                                                secureDataTansfer);
            return this;
        }

        /// <inheritdoc />
        public IArtifactExecutablePackageResourceBuilder MetaData(Action<IDefinitionMetaDataBuilder> action)
        {
            this._definitionMetaData = DefinitionMetaDataBuilder.Execute(action);
            return this;
        }

        #region Tools

        /// <summary>
        /// Normalizes a package relative path: '/' separators, without leading "./" or '/'
        /// </summary>
        private static string NormalizeRelativePath(string path)
        {
            var normalized = (path ?? string.Empty).Trim().Replace('\\', '/');

            while (normalized.StartsWith("./", StringComparison.Ordinal))
                normalized = normalized.Substring(2);

            return normalized.TrimStart('/');
        }

        /// <summary>
        /// Extracts the template files.
        /// </summary>
        private void ExtractTemplateFiles(Uri absolutePackageSource, IFileSystemHandler fileSystemHandler, IEnumerable<string> fileTemplates, bool recursive, HashSet<string> packageFiles)
        {
            foreach (var tpl in fileTemplates)
            {
                var files = fileSystemHandler.SearchFiles(absolutePackageSource.LocalPath, tpl, recursive);
                foreach (var file in files)
                {
                    var relative = absolutePackageSource.MakeRelativeUri(file);
                    packageFiles.Add(relative.OriginalString);
                }
            }
        }

        /// <inheritdoc />
        private void AppendFilesImpl(params string[] files)
        {
            foreach (var file in files)
                this._packageFiles.Add(file);
        }

        /// <inheritdoc />
        private void AppendFilesTemplateImpl(string fileTemplate, bool recursive)
        {
            ArgumentNullException.ThrowIfNull(fileTemplate);

            if (recursive)
                this._packageRecurciveTemplateFiles.Add(fileTemplate);
            else
                this._packageTemplateFiles.Add(fileTemplate);
        }

        /// <summary>
        /// Excludes all files path that correspond to following regex
        /// </summary>
        private void ExcludePathRegexImpl(Regex regex)
        {
            this._packageExcludeReg.Add(regex);
        }

        /// <summary>
        /// Excludes all files
        /// </summary>
        private void ExcludeFilesImpl(params string[] files)
        {
            foreach (var file in files)
                this._packageExcludeFiles.Add(file);
        }

        /// <summary>
        /// Excludes all files of the directories.
        /// </summary>
        private void ExcludeDirectoriesImpl(params string[] directories)
        {
            foreach (var dir in directories)
                this._packageExcludeDir.Add(dir);
        }

        #endregion

        #endregion
    }
}
