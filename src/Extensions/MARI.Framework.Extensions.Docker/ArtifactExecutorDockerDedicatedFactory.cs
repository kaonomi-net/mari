namespace MARI.Framework.Extensions.Docker
{
    using MARI.Framework.Cluster.Abstractions.Services;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Artifacts;
    using MARI.Framework.Extensions.Docker.Abstractions;
    using MARI.Framework.Extensions.Docker.Abstractions.Models;
    using MARI.Framework.Node.Abstractions.Artifacts;
    using MARI.Framework.Node.Artifacts;

    using MARI.Toolbox.Abstractions.Services;
    using MARI.Toolbox.Extensions;

    using global::Docker.DotNet;
    using global::Docker.DotNet.Models;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    using System;
    using System.Diagnostics;
    using System.Formats.Tar;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Factory used to provide artifact executor using docker
    /// </summary>
    internal sealed class ArtifactExecutorDockerDedicatedFactory : ArtifactExecutorBaseDedicatedFactory, IArtifactExecutorDedicatedFactory
    {
        #region Fields

        private static readonly string s_template;

        // [registry[:port]/]name[/name...][@digest]
        private static readonly Regex s_imageNameValidator = new Regex("^[A-Za-z0-9][A-Za-z0-9._\\-/:@]*$", RegexOptions.Compiled);
        private static readonly Regex s_imageTagValidator = new Regex("^[A-Za-z0-9_][A-Za-z0-9_.\\-]{0,127}$", RegexOptions.Compiled);
        private readonly IConfiguration _configuration;
        private readonly IProcessSystemService _processSystemService;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly INetworkInspector _networkInspector;
        private readonly IDockerProcessorFactory _dockerProcessorFactory;
        private readonly IFileSystemHandler _localFileSystemHandler;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ArtifactExecutorDockerDedicatedFactory"/> class.
        /// </summary>
        static ArtifactExecutorDockerDedicatedFactory()
        {
            var assembly = typeof(ArtifactExecutorDockerDedicatedFactory).Assembly;
            var fullName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("GrainDockerfile.template", StringComparison.OrdinalIgnoreCase));

            Debug.Assert(fullName != null);

            using (var stream = assembly.GetManifestResourceStream(fullName)!)
            using (var reader = new StreamReader(stream))
            {
                s_template = reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactExecutorDockerDedicatedFactory"/> class.
        /// </summary>
        public ArtifactExecutorDockerDedicatedFactory(IProcessSystemService processSystemService,
                                                      IJsonSerializer jsonSerializer,
                                                      INetworkInspector networkInspector,
                                                      IFileSystemHandler fileSystemHandler,
                                                      IConfiguration configuration,
                                                      IDockerProcessorFactory dockerProcessorFactory)
            : base(fileSystemHandler)
        {
            this._configuration = configuration;
            this._processSystemService = processSystemService;
            this._jsonSerializer = jsonSerializer;
            this._networkInspector = networkInspector;
            this._dockerProcessorFactory = dockerProcessorFactory;

            this._localFileSystemHandler = fileSystemHandler;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool CanManaged(ArtifactExecutableDefinition artifactExecutableDefinition)
        {
            return artifactExecutableDefinition is not null &&
                   artifactExecutableDefinition.Environment is ArtifactExecutableDockerEnvironmentDefinition;
        }

        /// <inheritdoc />
        public override async ValueTask<IArtifactExternalCodeExecutor> BuildNewExecutorAsync(ArtifactExecutableDefinition definition,
                                                                                    IArtifactExternalCodeExecutor? previousExecutor,
                                                                                    IExecutionContext executionContext,
                                                                                    ILogger logger,
                                                                                    CancellationToken token)
        {
            IArtifactExternalCodeExecutor? executor;

            // Build image if needed
            await EnsureArtifactImageIsAvailableAsync(definition, (ArtifactExecutableDockerEnvironmentDefinition)definition.Environment!, logger, token);

            token.ThrowIfCancellationRequested();

            if (definition.AllowPersistence)
            {
                executor = new ExternalDockerCodeDaemonExecutor(definition,
                                                                this._processSystemService,
                                                                this._jsonSerializer,
                                                                this._networkInspector,
                                                                this._dockerProcessorFactory,
                                                                this._configuration);
            }
            else
            {
                executor = new ExternalDockerCodeCLIExecutor(definition,
                                                             this._processSystemService,
                                                             this._jsonSerializer,
                                                             this._configuration,
                                                             this._dockerProcessorFactory);
            }

            return executor!;
        }

        /// <inheritdoc />
        public override async ValueTask<bool> CheckExecutorValidityAsync(ArtifactExecutableDefinition artifactExecutableDefinition,
                                                                         IArtifactExternalCodeExecutor artifact,
                                                                         IExecutionContext executionContext,
                                                                         ILogger logger,
                                                                         CancellationToken token)
        {
            if (artifact is null || artifactExecutableDefinition.AllowPersistence == false)
                return false;

            if (artifact is ExternalCodeDeamonExecutor dockerExecutor &&
                dockerExecutor.Processor is not null)
            {
                return await ((DockerExternalProcess)dockerExecutor.Processor).IsAliveAsync(artifactExecutableDefinition, token);
            }

            return false;
        }

        #region Tools

        /// <summary>
        /// Ensures the artifact image is available asynchronous.
        /// </summary>
        private async Task EnsureArtifactImageIsAvailableAsync(ArtifactExecutableDefinition definition,
                                                               ArtifactExecutableDockerEnvironmentDefinition env,
                                                               ILogger logger,
                                                               CancellationToken token)
        {
            if (definition.PackageType == ArtifactPackageTypeEnum.EnvironmentRepository)
                return;

            using (var client = new DockerClientConfiguration()
                                            .CreateClient(string.IsNullOrEmpty(env.MinimalRequiredVersion)
                                                                    ? null
                                                                    : System.Version.Parse(env.MinimalRequiredVersion)))
            {
                var imgName = (definition.Uid.ToString() + ":" + definition.Hash).ToLowerInvariant();

                ImageInspectResponse? img = null;
                try
                {
                    img = await client.Images.InspectImageAsync(definition.Uid.ToString(), token);

                    if (img is not null &&
                        img.Config?.Labels is not null &&
                        img.Config.Labels.TryGetValue("hash", out var containerHash) &&
                        containerHash == definition.Hash)
                    {
                        return;
                    }
                }
                catch
                {
                }

                if (img is not null)
                    await client.Images.DeleteImageAsync(img.ID, new ImageDeleteParameters() { Force = true });

                var rootPath = await base.GetLocalFolderPathAsync(definition, token);
                var dockerfileBuild = await CompileDockerFile(definition, env, logger, token, rootPath);

                var dockerfileBytes = Encoding.UTF8.GetBytes(dockerfileBuild);

                var file = new Uri(rootPath, "Dockerfile");
                await this._localFileSystemHandler.WriteToFileAsync(dockerfileBytes, file, true, token);

                using (var memory = new MemoryStream())
                {
                    await TarFile.CreateFromDirectoryAsync(rootPath.OriginalString, memory, false, token);

                    await CleanUpLocalFolderAsync(definition, token);

                    memory.Seek(0, SeekOrigin.Begin);

                    var errors = new List<string>();

                    var progress = new Progress<JSONMessage>();
                    progress.ProgressChanged += (e, a) =>
                    {
                        if (!string.IsNullOrEmpty(a.ErrorMessage))
                        {
                            errors.Add(a.ErrorMessage);
                            logger.OptiLog(LogLevel.Error, "Docker image build {From} = {Error}:{ErrorMessage}", a.From, a.Error, a.ErrorMessage);
                        }
                        else if (!string.IsNullOrEmpty(a.ProgressMessage))
                        {
                            logger.OptiLog(LogLevel.Information, "Docker image build {message}", a.ProgressMessage);
                        }
                    };

                    await client.Images.BuildImageFromDockerfileAsync(new ImageBuildParameters()
                    {
                        Tags = new List<string>() { definition.Uid.ToString() + ":latest" },
                        Labels = new Dictionary<string, string>()
                        {
                            { "hash", definition.Hash }
                        },
                    }, memory, null, null, progress, token);

                    if (errors.Count > 0)
                        throw new InvalidDataException("[Artifact:" + definition.Uid + "] Docker Image build from artifact failed\n" + string.Join(Environment.NewLine, errors));
                }
            }
        }

        /// <summary>
        /// Compiles the docker file.
        /// </summary>
        private async ValueTask<string> CompileDockerFile(ArtifactExecutableDefinition definition,
                                                          ArtifactExecutableDockerEnvironmentDefinition env,
                                                          ILogger logger,
                                                          CancellationToken token,
                                                          Uri rootPath)
        {
            var copyDockerfileBuilder = new StringBuilder();

            var files = new List<Uri>();

            foreach (var f in definition.PackageFiles)
            {
                var dockerPath = ToSafeDockerfilePath(f, definition);

                copyDockerfileBuilder.Append("COPY [\"");
                copyDockerfileBuilder.Append(dockerPath);
                copyDockerfileBuilder.Append("\", \"");
                copyDockerfileBuilder.Append(dockerPath);
                copyDockerfileBuilder.AppendLine("\"]");

                files.Add(new Uri(rootPath, f));
            }

            await InstallInLocalAsync(definition, rootPath, files, logger, null, token);

            if (string.IsNullOrEmpty(env.Image) || !s_imageNameValidator.IsMatch(env.Image))
                throw new InvalidDataException("[Artifact:" + definition.Uid + "] Invalid docker image name '" + env.Image + "'");

            if (!string.IsNullOrEmpty(env.Tag) && !s_imageTagValidator.IsMatch(env.Tag))
                throw new InvalidDataException("[Artifact:" + definition.Uid + "] Invalid docker image tag '" + env.Tag + "'");

            // ExtraDockerFileInstructions are by design raw dockerfile instructions provided by the artifact definition author
            return s_template.Replace("<<PARENT_IMAGE>>", env.Image + ":" + (string.IsNullOrEmpty(env.Tag) ? "latest" : env.Tag))
                             .Replace("<<COPY_FILES>>", copyDockerfileBuilder.ToString())
                             .Replace("<<EXTR_BUILD_INSTRUCTION>>", string.Join(Environment.NewLine, env.ExtraDockerFileInstructions));
        }

        /// <summary>
        /// Validate a package file path before inserting it in a dockerfile JSON-array instruction
        /// (forbid characters able to escape the json string or the dockerfile line)
        /// </summary>
        private static string ToSafeDockerfilePath(string file, ArtifactExecutableDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(file) || file.Any(c => c == '"' || char.IsControl(c)))
                throw new InvalidDataException("[Artifact:" + definition.Uid + "] Invalid package file name '" + file?.ReplaceLineEndings(" ") + "'");

            // Docker build context paths use '/' as separator; a '\\' would be interpreted as a json escape sequence
            return file.Replace('\\', '/');
        }

        #endregion

        #endregion
    }
}
