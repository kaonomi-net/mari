// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders
{
    using MARI.Framework.Builders.Models;
    using MARI.Framework.Builders.Sequences;
    using MARI.Framework.Builders.Steps;
    using MARI.Framework.Builders.Triggers;
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Enums;
    using MARI.Framework.Core.Abstractions.Inputs;
    using MARI.Framework.Core.Abstractions.References;
    using MARI.Framework.Core.Abstractions.Sequence.Stages;
    using MARI.Framework.Core.Abstractions.Signals;
    using MARI.Framework.Core.Abstractions.Streams;

    using MARI.Toolbox;
    using MARI.Toolbox.Abstractions.Expressions;
    using MARI.Toolbox.Abstractions.Helpers;
    using MARI.Toolbox.Abstractions.Models;
    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Extensions;
    using MARI.Toolbox.Helpers;
    using MARI.Toolbox.Loggers;
    using MARI.Toolbox.Models;
    using MARI.Toolbox.Serializations;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;

    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    /// <summary>
    /// Compiler to transform YAML description to mari <see cref="IDefinition"/>
    /// </summary>
    /// <seealso cref="IDefinitionCompiler" />
    internal sealed class DefinitionCompiler : IDefinitionCompiler
    {
        #region Fields

        private static readonly MethodInfo s_genericCompute;
        private static readonly MethodInfo s_requireInput;

        private static readonly Regex s_guidRegex;

        private static readonly IDeserializer s_yamlDeserializer;
        private static readonly ISerializer s_toJsonSerializer;
        private static readonly JSchema s_definitionSchema;

        private static readonly MethodInfo s_createSelectStage;

        /// <summary>
        /// Maximum number of characters accepted for a definition content
        /// </summary>
        private const int MAX_CONTENT_LENGTH = 4 * 1024 * 1024;

        /// <summary>
        /// Maximum nesting depth accepted for a definition content (yaml or json)
        /// </summary>
        private const int MAX_CONTENT_DEPTH = 64;

        private readonly IMARIReferenceSolverService _solverService;
        private readonly ILogger<IDefinitionCompiler> _logger;
        private readonly IObjectConverter _objectConverter;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DefinitionCompiler"/> class.
        /// </summary>
        static DefinitionCompiler()
        {
            s_guidRegex = new Regex("^" + RegexHelper.Pattern.GUID + "$", RegexOptions.IgnoreCase);

            var assembly = typeof(DefinitionCompiler).Assembly;

            s_requireInput = ((MethodCallExpression)((LambdaExpression)((DefinitionCompiler e) => e.ComputeSequenceRequireInputStep<NoneType>(null))).Body).Method.GetGenericMethodDefinition();

            Expression<Func<DefinitionCompiler, ValueTask<ISequencePipelineBaseBuilder>>> expr = e => e.ComputeSequenceStep<ISequencePipelineBaseBuilder, NoneType>(null!, null!, null!, null!);
            s_genericCompute = ((MethodCallExpression)expr.Body).Method.GetGenericMethodDefinition();

            Expression<Func<DefinitionCompiler, ISequencePipelineStageDefinitionProvider>> createSelectExpr = (DefinitionCompiler e) => e.CreateSelectStage<string, string>(null!);
            s_createSelectStage = ((MethodCallExpression)createSelectExpr.Body).Method.GetGenericMethodDefinition();

            var fullSchemaName = assembly.GetManifestResourceNames().First(n => n.EndsWith("definitions.schema.json"));
            using (var stream = assembly.GetManifestResourceStream(fullSchemaName))
            using (var reader = new StreamReader(stream!))
            {
                s_definitionSchema = JSchema.Parse(reader.ReadToEnd());
            }

            s_yamlDeserializer = new DeserializerBuilder().WithAttemptingUnquotedStringTypeDeserialization()
                                                          .Build();

            s_toJsonSerializer = new SerializerBuilder().JsonCompatible()
                                                        .Build();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionCompiler"/> class.
        /// </summary>
        public DefinitionCompiler(IMARIReferenceSolverService solverService,
                                  IObjectConverter objectConverter,
                                  ILogger<IDefinitionCompiler>? logger = null)
        {
            this._solverService = solverService;
            this._objectConverter = objectConverter;
            this._logger = logger ?? NullLogger<IDefinitionCompiler>.Instance;
        }

        #endregion

        #region nested

        /// <summary>
        /// Yaml parser wrapper that rejects aliases (prevent alias expansion attacks) and limits the nesting depth
        /// </summary>
        private sealed class SafeYamlParser : IParser
        {
            #region Fields

            private readonly IParser _parser;
            private int _depth;

            #endregion

            #region Ctor

            /// <summary>
            /// Initialize a new instance of the class <see cref="SafeYamlParser"/>
            /// </summary>
            public SafeYamlParser(IParser parser)
            {
                this._parser = parser;
            }

            #endregion

            #region Properties

            /// <inheritdoc />
            public ParsingEvent? Current
            {
                get { return this._parser.Current; }
            }

            #endregion

            #region Methods

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (!this._parser.MoveNext())
                    return false;

                var current = this._parser.Current;

                if (current is AnchorAlias)
                    throw new InvalidDataException("Yaml aliases are not supported in definitions (" + current.Start + ")");

                if (current is MappingStart || current is SequenceStart)
                {
                    this._depth++;
                    if (this._depth > MAX_CONTENT_DEPTH)
                        throw new InvalidDataException("Yaml definition exceed the maximum depth allowed " + MAX_CONTENT_DEPTH);
                }
                else if (current is MappingEnd || current is SequenceEnd)
                {
                    this._depth--;
                }

                return true;
            }

            #endregion
        }

        private sealed class CompilationContext : SafeDisposable
        {
            #region Fields

            private readonly Dictionary<string, HashSet<(IDefinition Def, Uri RefId, string NamespaceId, RefTypeEnum Type)>> _indexedDefinitions;

            private readonly List<IDefinition> _definitions;
            private readonly RelayLogger _relayLogger;

            #endregion

            #region Ctor

            /// <summary>
            /// Initialize a new instance of the class <see cref="CompilationContext"/>
            /// </summary>
            public CompilationContext(ILogger logger, CancellationToken token)
            {
                this._indexedDefinitions = new Dictionary<string, HashSet<(IDefinition Def, Uri RefId, string NamespaceId, RefTypeEnum Type)>>();
                this._relayLogger = new RelayLogger(logger, "compilation", true);
                this._definitions = new List<IDefinition>();

                this.CancellationToken = token;
            }

            #endregion

            #region Properties

            /// <summary>
            /// 
            /// </summary>
            public ILogger Logger
            {
                get { return this._relayLogger; }
            }

            /// <summary>
            /// 
            /// </summary>
            public CancellationToken CancellationToken { get; }

            #endregion

            #region Method

            /// <summary>
            /// 
            /// </summary>
            public void PushDefinition(IDefinition definition)
            {
                this._definitions.Add(definition);

                if (definition is IRefDefinition refDef)
                {
                    RefIdHelper.Explode(refDef.RefId, out var type, out var @namespace, out var sni);

                    HashSet<(IDefinition Def, Uri RefId, string NamespaceId, RefTypeEnum Type)>? defs = null;
                    if (!this._indexedDefinitions.TryGetValue(sni, out defs))
                    {
                        defs = new HashSet<(IDefinition Def, Uri RefId, string NamespaceId, RefTypeEnum Type)>();
                        this._indexedDefinitions.Add(sni, defs);
                    }

                    defs.Add((definition, refDef.RefId, @namespace, type));
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public CompilationResult Close()
            {
                bool success = true;
                foreach (var finalDefinitions in this._definitions)
                {
                    success &= finalDefinitions.Validate(this._relayLogger);
                }

                var logError = this._relayLogger.GetLogsCopy();

                return new CompilationResult(success, logError, null, this._definitions);
            }

            /// <summary>
            /// 
            /// </summary>
            public IReadOnlyCollection<IDefinition> SolveDefinitionReference(Uri refIdUri)
            {
                RefIdHelper.Explode(refIdUri, out var type, out var @namespace, out var sni);

                if (this._indexedDefinitions.TryGetValue(sni, out var defs))
                {
                    return defs.Where(d =>
                    {
                        if (type != RefTypeEnum.None && d.Type != type)
                            return false;

                        if (string.Equals(@namespace, RefIdHelper.DEFAULT_NAMESPACE) == false && string.Equals(@namespace, d.NamespaceId) == false)
                            return false;

                        return true;
                    })
                    .Select(d => d.Def)
                    .ToArray();
                }

                return EnumerableHelper<IDefinition>.ReadOnly;
            }

            /// <inheritdoc />
            protected override void DisposeBegin()
            {
                this._relayLogger.Dispose();
                base.DisposeBegin();
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask<CompilationResult> CompileAsync(Stream content,
                                                               DefinitionParserSourceEnum sourceType = DefinitionParserSourceEnum.Yaml,
                                                               ILogger? logger = null,
                                                               CancellationToken token = default)
        {
            using (var reader = new StreamReader(content))
            {
                var str = await reader.ReadToEndAsync();
                return await CompileAsync(str, sourceType, logger, token);
            }
        }

        /// <inheritdoc />
        public async ValueTask<CompilationResult> CompileAsync(string content,
                                                               DefinitionParserSourceEnum sourceType = DefinitionParserSourceEnum.Yaml,
                                                               ILogger? logger = null,
                                                               CancellationToken token = default)
        {
            var def = DeserializaDefinitionFile(content, sourceType);

            if (def is null)
                throw new InvalidDataException("Couldn't load yaml definitions");

            using (var ctx = new CompilationContext(logger ?? this._logger, token))
            {
                foreach (var sgnlDef in def.Signals ?? EnumerableHelper<SignalDef>.ReadOnly)
                    ctx.PushDefinition(ComputeSignal(sgnlDef, def.Global));

                foreach (var streamDef in def.Streams ?? EnumerableHelper<StreamQueueDef>.ReadOnly)
                    ctx.PushDefinition(ComputeStreamQueue(streamDef, def.Global));

                foreach (var sequence in def.Sequences ?? EnumerableHelper<SequenceDef>.ReadOnly)
                {
                    var seqDef = await ComputeSequence(sequence, def.Global, ctx);
                    ctx.PushDefinition(seqDef);
                }

                foreach (var triggerDef in def.Triggers ?? EnumerableHelper<TriggerBaseDef>.ReadOnly)
                {
                    var trgDef = await ComputeTrigger(triggerDef, def.Global, ctx);
                    ctx.PushDefinition(trgDef);
                }

                return ctx.Close();
            }
        }

        #region Tools

        /// <summary>
        /// Deserializas the definition file.
        /// </summary>
        /// <remarks>
        ///     1) Convert Yaml to Json if needed
        ///     2) Validate json from schema
        ///     3) load json into <see cref="LightDefinitions"/> object
        /// </remarks>
        private LightDefinitions? DeserializaDefinitionFile(string content, DefinitionParserSourceEnum sourceType)
        {
            ArgumentNullException.ThrowIfNull(content);

            if (content.Length > MAX_CONTENT_LENGTH)
                throw new InvalidDataException("Definition content exceed the maximum size allowed " + MAX_CONTENT_LENGTH + " characters");

            if (sourceType == DefinitionParserSourceEnum.Yaml)
            {
                // Convert yaml to json to apply the same validator schema
                content = ConvertYamlToJson(content);
            }

            JObject json;
            using (var jsonTxt = new StringReader(content))
            using (var jsonReader = new JsonTextReader(jsonTxt) { MaxDepth = MAX_CONTENT_DEPTH })
            {
                json = JObject.Load(jsonReader);

                // Same behavior as JObject.Parse : no additional content allowed
                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType != JsonToken.Comment)
                        throw new JsonReaderException("Additional text found in JSON definition after finishing reading.");
                }
            }
            if (!json.IsValid(s_definitionSchema))
            {
                using (var txt = new StringReader(content))
                using (var reader = new JsonTextReader(txt))
                using (var validatingReader = new JSchemaValidatingReader(reader))
                {
                    validatingReader.Schema = s_definitionSchema;

                    var serializer = new JsonSerializer();
                    // Call to get the error definitions
                    serializer.Deserialize(validatingReader);
                }
            }

            foreach (var obj in json.Descendants().OrderByDescending(d => d.Path.Length).ToArray())
            {
                if (obj is JProperty prop)
                {
                    if ((prop.Name == "cron" || prop.Name == "use" ||
                         prop.Name == "select" || prop.Name == "signal" ||
                         prop.Name == "stream" || prop.Name == "static") && prop.Parent is not null)
                    {
                        var firstParentProp = prop.Ancestors().OfType<JProperty>().FirstOrDefault();

                        if (firstParentProp is not null && (firstParentProp.Name == "stages" || firstParentProp.Name == "triggers"))
                        {
                            prop.Parent.Add(new JProperty("Type", prop.Name));
                        }
                        else if (firstParentProp is not null && (firstParentProp.Name == "output"))
                        {
                            var clone = prop.Value.DeepClone();

                            if (clone is JObject cloneContainer)
                            {
                                cloneContainer.Add(new JProperty("OutputType", prop.Name));
                                foreach (var cloneProp in cloneContainer.Properties().ToArray())
                                {
                                    prop.Parent.Add(cloneProp);
                                }
                            }
                            else
                            {
                                throw new InvalidDataException("Output '" + prop.Name + "' (" + prop.Path + ") must be an object");
                            }
                            prop.Remove();
                        }
                    }
                    else if (prop.Name.Contains("-") && prop.Parent is not null)
                    {
                        prop.Parent.Add(new JProperty(prop.Name.Replace("-", ""), prop.Value.DeepClone()));
                    }
                }
            }

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.Configure(b =>
            {
                b.Converter.For<TriggerBaseDef>().Where(t => t.Type, "cron").ApplyType<TriggerCronDef>()
                                                 .Where(t => t.Type, "signal").ApplyType<TriggerSignalDef>()
                                                 .Where(t => t.Type, "stream").ApplyType<TriggerStreamDef>()
                                                 .Done

                           .For<SequenceBaseStageDef>().Where(t => t.Type, "use").ApplyType<SequenceUseStageDef>()
                                                      .Where(t => t.Type, "select").ApplyType<SequenceSelectStageDef>()
                                                      .Done

                           .For<TriggerAbstractOutput>().Where(t => t.OutputType, "static").ApplyType<TriggerStaticCollectionOutput>();
            });

            var def = json.ToObject<LightDefinitions>(JsonSerializer.Create(serializerSettings));
            return def;
        }

        /// <summary>
        /// Converts the yaml to json.
        /// </summary>
        private string ConvertYamlToJson(string content)
        {
            object? yamlObject;
            using (var yamlTxt = new StringReader(content))
            {
                yamlObject = s_yamlDeserializer.Deserialize(new SafeYamlParser(new Parser(yamlTxt)));
            }

            var json = s_toJsonSerializer.Serialize(yamlObject);
            return json;
        }

        /// <summary>
        /// Computes the signal.
        /// </summary>
        private IDefinition ComputeSignal(SignalDef sgnlDef, GlobalOptionDefintion global)
        {
            return Signal.Create(sgnlDef.sni,
                                 sgnlDef.Signal,
                                 fixUid: sgnlDef.Uid,
                                 m => ComputeDefinitionMetaData(m, sgnlDef.MetaData, global));
        }

        /// <summary>
        /// Computes the stream queue.
        /// </summary>
        private IDefinition ComputeStreamQueue(StreamQueueDef def, GlobalOptionDefintion global)
        {
            return StreamQueue.Create(def.sni,
                                      string.IsNullOrEmpty(def.configName) ? StreamQueueDefinition.DEFAULT_STREAM_KEY : def.configName,
                                      def.QueueNamespace,
                                      def.QueueName,
                                      def.Uid,
                                      m => ComputeDefinitionMetaData(m, def.MetaData, global));
        }

        /// <summary>
        /// Compute trigger
        /// </summary>
        private async ValueTask<IDefinition> ComputeTrigger(TriggerBaseDef def, GlobalOptionDefintion global, CompilationContext ctx)
        {
            ITriggerDefinitionBuilder? trigger = null;
            TriggerAbstractOutput? output = null;

            if (def is TriggerCronDef cron)
            {
                output = cron.Output;
                trigger = Trigger.Cron(cron.Period,
                                       cron.sni,
                                       cron.sni,
                                       fixUid: cron.Uid,
                                       metadataBuilder: m => ComputeDefinitionMetaData(m, def.MetaData, global));
            }
            else if (def is TriggerSignalDef signal)
            {
                output = signal.Output;
                var refId = await SolveDefinitionReference(signal.From, RefTypeEnum.Signal, ctx);
                trigger = Trigger.Signal(new SignalId(refId, null),
                                         signal.sni,
                                         signal.sni,
                                         fixUid: signal.Uid,
                                         metadataBuilder: m => ComputeDefinitionMetaData(m, def.MetaData, global));
            }
            else if (def is TriggerStreamDef stream)
            {
                var refId = await SolveDefinitionReference(stream.From, RefTypeEnum.StreamQueue, ctx);
                var streamTrigger = Trigger.Stream(refId,
                                                   stream.sni,
                                                   stream.sni,
                                                   fixUid: stream.Uid,
                                                   metadataBuilder: m => ComputeDefinitionMetaData(m, def.MetaData, global));

                if (stream.MaxConsumerByNode is not null && stream.MaxConsumerByNode > 0)
                    trigger = streamTrigger.MaxConcurrentFactorClusterRelativeProcess((uint)stream.MaxConsumerByNode);
                else if (stream.MaxConsumer is not null && stream.MaxConsumer > 0)
                    trigger = streamTrigger.MaxConcurrentProcess((uint)stream.MaxConsumer);
                else
                    trigger = streamTrigger.MaxConcurrentFactorClusterRelativeProcess((uint)42);
            }
            else
            {
                throw new NotSupportedException("Trigger type now supported yet " + def);
            }

            if (output is not null && trigger is ITriggerDefinitionWithCustomOutput withInput)
                await ComputeTriggerOuput(withInput, output, ctx);

            ITriggerDefinitionFinalizeBuilder? triggerFinalizer = null;

            if (def.Targets is not null)
            {
                // ApplyTarget only record (and return a builder) when at least one target id is resolved
                triggerFinalizer = await ApplyTarget(def.Targets.Signals,
                                                     ctx,
                                                     RefTypeEnum.Signal,
                                                     ids => (triggerFinalizer ?? trigger).AddTargetSignals(ids)) ?? triggerFinalizer;

                triggerFinalizer = await ApplyTarget(def.Targets.Streams,
                                                     ctx,
                                                     RefTypeEnum.StreamQueue,
                                                     ids => (triggerFinalizer ?? trigger).AddTargetStreams(ids)) ?? triggerFinalizer;

                triggerFinalizer = await ApplyTarget(def.Targets.Sequences,
                                                     ctx,
                                                     RefTypeEnum.Sequence,
                                                     ids => (triggerFinalizer ?? trigger).AddTargetSequences(ids)) ?? triggerFinalizer;
            }

            if (triggerFinalizer is null)
                throw new InvalidDataException("A trigger must have at least ONE target");

            return triggerFinalizer.Build();
        }

        /// <summary>
        /// 
        /// </summary>
        private async Task<ITriggerDefinitionFinalizeBuilder?> ApplyTarget(IReadOnlyCollection<string>? refs,
                                                                            CompilationContext ctx,
                                                                            RefTypeEnum refType,
                                                                            Func<Guid[], ITriggerDefinitionFinalizeBuilder> record)
        {
            // Convert each ValueTask to Task only once (a ValueTask MUST NOT be consumed more than once)
            var signalTargetTasks = (refs ?? EnumerableHelper<string>.ReadOnly)
                                        .Select(s => SolveDefinitionReference(s, refType, ctx).AsTask())
                                        .ToArray();

            var signalTargets = await Task.WhenAll(signalTargetTasks).WaitAsync(ctx.CancellationToken);

            ctx.CancellationToken.ThrowIfCancellationRequested();

            if (signalTargets.Length == 0)
                return null;

            return record(signalTargets);
        }

        /// <summary>
        /// Computes the stream queue.
        /// </summary>
        private async ValueTask<IDefinition> ComputeSequence(SequenceDef def, GlobalOptionDefintion global, CompilationContext ctx)
        {
            if (def.Stages is null || def.Stages.Count == 0)
                throw new InvalidOperationException("Compute a sequence requirement Step : " + def.Sequence);

            var sequenceBuilder = Sequence.Build(def.sni,
                                                 def.sni,
                                                 def.Uid,
                                                 o => o.PreventSequenceExecutorStateStorage(),
                                                 m => ComputeDefinitionMetaData(m, def.MetaData, global));

            var requiredInputType = NoneType.Trait;

            if (!string.IsNullOrEmpty(def.RequiredInput))
            {
                var inputRefId = GetRefId(def.RequiredInput, RefTypeEnum.Type);
                var previousInputRef = await this._solverService.GetReferenceTypeAsync(inputRefId);

                ctx.CancellationToken.ThrowIfCancellationRequested();

                if (previousInputRef?.Item1 is null)
                    throw new InvalidDataException("Sequence required input not founded : " + def.Sequence + " require-input " + inputRefId);

                requiredInputType = previousInputRef.Item1;
            }

            var pipeline = (ISequencePipelineBaseBuilder)s_requireInput.MakeGenericMethod(requiredInputType).Invoke(this, new object[] { sequenceBuilder })!;

            var previousInput = requiredInputType!;
            foreach (var step in def.Stages)
            {
                pipeline = await (ValueTask<ISequencePipelineBaseBuilder>)s_genericCompute.MakeGenericMethod(pipeline.GetType(), previousInput)
                                                                                          .Invoke(this, new object[] { step, pipeline, def, global })!;

                ctx.CancellationToken.ThrowIfCancellationRequested();

                var builderType = pipeline.GetType().GetAllCompatibleAbstractTypes()
                                                    .OfType<ConcretType>()
                                                    .Where(a => a.DisplayName.StartsWith(nameof(ISequencePipelineBuilder) + "<") &&
                                                                a.IsGenericComposed &&
                                                                a.GenericParameters.Count == 1)
                                                    .FirstOrDefault();

                previousInput = builderType?.GenericParameters.First().ToType() ?? NoneType.Trait;
            }

            return pipeline.Build();
        }

        /// <summary>
        /// Pres the compute sequence step.
        /// </summary>
        private async ValueTask<ISequencePipelineBaseBuilder> ComputeSequenceStep<TPipeline, TInput>(SequenceBaseStageDef step,
                                                                                                     TPipeline pipeline,
                                                                                                     SequenceDef def,
                                                                                                     GlobalOptionDefintion global)
            where TPipeline : ISequencePipelineBaseBuilder
        {
            if (step is SequenceUseStageDef useStepDef)
                return await ComputeSequenceUseStep<TPipeline, TInput>(useStepDef, pipeline);

            if (step is SequenceSelectStageDef selectStepDef)
                return await ComputeSequenceSelectStep<TPipeline, TInput>(selectStepDef, pipeline);

            throw new NotSupportedException("Step pre-compute is not implemented yet");
        }

        /// <summary>
        /// Pres the compute sequence use step.
        /// </summary>
        private async ValueTask<ISequencePipelineBaseBuilder> ComputeSequenceUseStep<TPipeline, TInput>(SequenceUseStageDef useStepDef, TPipeline pipeline)
            where TPipeline : ISequencePipelineBaseBuilder
        {
            var vgrainTargetRefId = GetRefId(useStepDef.Use, RefTypeEnum.VGrain);
            var grainType = await this._solverService.GetReferenceTypeAsync(vgrainTargetRefId);

            if (grainType is null || grainType.Item1 is null)
                throw new InvalidDataException("sequence step vgrain not founded " + useStepDef.Use);

            var method = await this._solverService.GetReferenceMethodAsync(RefIdHelper.WithMethod(grainType.Item2, useStepDef.Call), grainType.Item1);

            if (method is null)
                throw new KeyNotFoundException("Could not found method on " + grainType + " with SNI " + useStepDef.Call);

            // use variable because later on support input to be 'input.prop.prop'
            var inpuArgType = typeof(TInput);
            var execConfigType = typeof(IExecutionContext);

            AccessExpressionDefinition? configurationAcccess = null;
            ConcretType? configurationContextSource = null;

            if (useStepDef.Config is not null)
            {
                var access = await CompileAccessFrom<TInput>(useStepDef.Config.Type, useStepDef.Config.Mode, useStepDef.Config.From);

                if (access is not null && access.TargetType != NoneType.AbstractTrait)
                    execConfigType = typeof(IExecutionContext<>).MakeGenericTypeWithCache(access.TargetType.ToType());

                //configurationContextSource = inpuArgType.GetAbstractType() as ConcretType;
                configurationAcccess = access;
            }

            var parameters = new List<SequenceStageCallParameterDefinition>(2);

            var mthParameters = method.GetParameters();

            var execContextParameter = mthParameters.LastOrDefault();
            var paramArgs = mthParameters.Length > 1 ? mthParameters.First() : null;


            if (execContextParameter is null || !execContextParameter.ParameterType.IsAssignableTo(typeof(IExecutionContext)))
                throw new InvalidDataException("A method used sequence MUST at least take an IExecutionContext in parameter");

            // Only (ctx) and (input, ctx) method shapes are supported
            if (mthParameters.Length > 2)
                throw new InvalidDataException("A method used in sequence MUST take at most two parameters (input, IExecutionContext) : " + useStepDef.Use + "." + useStepDef.Call + " take " + mthParameters.Length);

            if (method.IsGenericMethod)
                method = method.MakeGenericMethodFromParameters(new Type[] { inpuArgType, execConfigType });

            var displayName = useStepDef.Use + "." + useStepDef.Call;
            if (mthParameters.Length == 2)
            {
                if (NoneType.IsEqualTo<TInput>())
                    throw new InvalidOperationException("Method " + method.Name + " Expect input value but none is provide (Step : " + displayName + ")");

                var conventionInputParam = Expression.Parameter(typeof(TInput), "i");
                parameters.Add(new SequenceStageCallParameterDefinition(parameters.Count,
                                                                        paramArgs?.Name ?? "i",
                                                                        Expression.Lambda(conventionInputParam, conventionInputParam).CreateAccess()));
            }

            var conventionCtxParamExecutionCtx = Expression.Parameter(execConfigType, "ctx");
            parameters.Add(new SequenceStageCallParameterDefinition(parameters.Count,
                                                                    execContextParameter.Name ?? "ctx",
                                                                    Expression.Lambda(conventionCtxParamExecutionCtx, conventionCtxParamExecutionCtx).CreateAccess()));

            var abstrOutput = method.ReturnType.GetAbstractType() as ConcretType;

            Debug.Assert(abstrOutput is not null, "Generic output MUST be solved FIRST");

            var output = abstrOutput.IsGenericComposed
                                    ? (Type?)abstrOutput.GenericParameters.First().ToType()
                                    : null;

            var builder = new CallStepBuilder(NoneType.IsEqualTo<TInput>() ? null : typeof(TInput),
                                              parameters.ToArray(),
                                              grainType.Item1,
                                              method,
                                              output);

            return pipeline.EnqueueStage(new DirectCallStepBuilder(builder,
                                                                   null,
                                                                   Guid.NewGuid(),
                                                                   displayName,
                                                                   output is null || output == NoneType.Trait,
                                                                   configurationAcccess,
                                                                   configurationContextSource), output);
        }

        /// <summary>
        /// Pres the compute sequence use step.
        /// </summary>
        private async ValueTask<ISequencePipelineBaseBuilder> ComputeSequenceSelectStep<TPipeline, TInput>(SequenceSelectStageDef stepDef, TPipeline pipeline)
            where TPipeline : ISequencePipelineBaseBuilder
        {
            var access = await CompileAccessFrom<TInput>(stepDef.Select, stepDef.Mode, stepDef.From);

            if (access == null)
                throw new InvalidOperationException("could not compile select step " + stepDef.Select);

            var output = access.TargetType.ToType();

            var stage = (ISequencePipelineStageDefinitionProvider)s_createSelectStage.MakeGenericMethod(typeof(TInput), output)
                                                                                     .Invoke(this, new object?[] { access })!;

            return pipeline.EnqueueStage(stage, output);
        }

        /// <summary>
        /// 
        /// </summary>
        private async Task ComputeTriggerOuput(ITriggerDefinitionWithCustomOutput withInput, TriggerAbstractOutput output, CompilationContext ctx)
        {
            DataSourceDefinition? dataDef = null;
            if (output is TriggerStaticCollectionOutput staticColl)
            {
                var valueType = GetRefId(staticColl.Type, RefTypeEnum.Type);
                var valueTypeRef = await this._solverService.GetReferenceTypeAsync(valueType);

                if (valueTypeRef is null || valueTypeRef.Item1 is null)
                    throw new KeyNotFoundException(string.Format("Data type not founded ref: {0}", valueType));

                var listValues = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericTypeWithCache(valueTypeRef.Item1))!;

                foreach (var value in staticColl.Values ?? Enumerable.Empty<string>() /* values may be missing from deserialized definition */)
                {
                    var converted = this._objectConverter.TryConvert(value, valueTypeRef.Item1, out var result);
                    if (converted == false)
                    {
                        ctx.Logger.OptiLog(LogLevel.Error,
                                           "Trigger Output static value conversion failed '{content}' type {type}",
                                           value,
                                           valueTypeRef.Item1);
                        continue;
                    }

                    listValues.Add(result);
                }

                var dataSourceType = typeof(DataSourceStaticCollectionDefinition<>).MakeGenericTypeWithCache(valueTypeRef.Item1);
                dataDef = (DataSourceDefinition)Activator.CreateInstance(dataSourceType,
                                                                         new object?[] { Guid.NewGuid(), listValues, staticColl.Mode, null })!;
            }

            if (dataDef is null)
                throw new NotSupportedException("Output type not managed " + output);

            withInput.SetOutput(dataDef);
        }

        /// <summary>
        /// Validate if <paramref name="value"/> match requirement <paramref name="parameter"/>
        /// </summary>
        private static bool ParameterMatch(ParameterInfo parameter, (Type type, object Value, bool IsRef) value)
        {
            if (parameter.ParameterType == value.type)
                return true;

            if (parameter.ParameterType.IsAssignableFrom(value.type))
                return true;

            // Numeric values are always parsed as double, allow explicit numeric conversion
            return IsNumericType(parameter.ParameterType) && IsNumericType(value.type);
        }

        /// <summary>
        /// Determines whether <paramref name="type"/> is a numeric primitive type (nullable included)
        /// </summary>
        private static bool IsNumericType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type.IsEnum)
                return false;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Transform the list of args in type and real access
        /// </summary>
        private static IReadOnlyList<(Type type, Expression Value, bool IsRef)> CompileArgs<TInput>(IReadOnlyCollection<string> fromInput, params ParameterExpression[] parameterExpressions)
        {
            var namedParameters = parameterExpressions.ToDictionary(p => p.Name ?? string.Empty);

            var result = new List<(Type type, Expression Value, bool IsRef)>(fromInput.Count);

            foreach (var parameter in fromInput)
            {
                ReadOnlySpan<char> paramSpan = parameter;
                paramSpan = paramSpan.Trim();
                if (paramSpan.StartsWith("ref/", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add((typeof(Uri), Expression.Constant(GetRefId(parameter, RefTypeEnum.Other)), true));
                    continue;
                }
                else if (s_guidRegex.IsMatch(parameter.Trim()))
                {
                    result.Add((typeof(Guid), Expression.Constant(Guid.Parse(parameter)), false));
                    continue;
                }

                var added = false;
                foreach (var p in namedParameters)
                {
                    if (paramSpan.StartsWith(p.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        var accessLambda = DynamicCallHelper.CompileCallChainAccess(p.Value, paramSpan.ToString(), true, true);
                        result.Add((accessLambda.ReturnType, accessLambda.Body, false));
                        added = true;
                        break;
                    }
                }

                if (!added)
                {
                    if (double.TryParse(paramSpan, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                    {
                        result.Add((typeof(double), Expression.Constant(number), false));
                        added = true;
                    }
                    else
                    {
                        result.Add((typeof(string), Expression.Constant(parameter.Trim()), false));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// solve definition reference
        /// </summary>
        private async ValueTask<Guid> SolveDefinitionReference(string refStr, RefTypeEnum refType, CompilationContext ctx)
        {
            if (Guid.TryParse(refStr.Trim().Trim('"').Trim('\''), out var refGuid))
                return refGuid;

            if (refStr.StartsWith("ref/", StringComparison.OrdinalIgnoreCase))
            {
                var refIdUri = GetRefId(refStr, refType);

                var definitions = ctx.SolveDefinitionReference(refIdUri);

                if (definitions is null || definitions.Any() == false)
                    definitions = await this._solverService.GetReferenceDefinitionsAsync(refIdUri);

                if (definitions is not null && definitions.Any())
                {
                    if (definitions.Count > 1)
                        throw new InvalidDataException("Multi definition match the reference " + refIdUri + ", try adding more information like namespaces.");

                    return definitions.First().Uid;
                }
            }

            throw new InvalidDataException("Could not resolved the reference " + refStr + " type " + refType);
        }

        /// <summary>
        /// Gets the reference identifier.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Uri GetRefId(string refId, RefTypeEnum type)
        {
            ReadOnlySpan<char> refStr = refId;
            return GetRefId(refStr, type);
        }

        /// <summary>
        /// Gets the reference identifier.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Uri GetRefId(ReadOnlySpan<char> refStr, RefTypeEnum type)
        {

            if (refStr.StartsWith("ref/", StringComparison.OrdinalIgnoreCase))
                refStr = refStr.Slice(4);

            var separator = refStr.IndexOf('/');
            if (separator == -1)
                separator = refStr.IndexOf('@');

            var namespaceIdentifier = string.Empty;
            if (separator > -1)
                namespaceIdentifier = refStr.Slice(0, separator).ToString();

            separator++;
            var sni = refStr.Slice(separator).ToString();

            return RefIdHelper.Generate(type, sni, namespaceIdentifier);
        }

        /// <summary>
        /// Computes the sequence require input step.
        /// </summary>
        private ISequencePipelineBuilder<TInput> ComputeSequenceRequireInputStep<TInput>(ISequenceTriggerBuilder triggerBuilder)
        {
            return triggerBuilder.RequiredInput<TInput>();
        }

        /// <summary>
        /// Computes the definition meta data.
        /// </summary>
        private void ComputeDefinitionMetaData(IDefinitionMetaDataBuilder m, MetaDataDef? metaData, GlobalOptionDefintion global)
        {
            var n = metaData?.Namespace ?? global?.Namespace;

            if (!string.IsNullOrEmpty(n))
                m.Namespace(n);

            if (metaData is null)
                return;

            if (metaData.Tags is not null && metaData.Tags.Any())
                m.AddTags(metaData.Tags);

            if (!string.IsNullOrEmpty(metaData.Category))
                m.CategoryPath(metaData.Category);

            if (!string.IsNullOrEmpty(metaData.Description))
                m.Description(metaData.Description);
        }

        #region Specific Sequence Stage Creation

        /// <summary>
        /// Create <see cref="SequencePipelineSelectStageBuilder{TInput, TSelect}"/>
        /// </summary>
        /// <remarks>
        ///     This method is to ensure any ctor change will be detected
        /// </remarks>
        private ISequencePipelineStageDefinitionProvider CreateSelectStage<TInput, TSelect>(AccessExpressionDefinition accessExpressionDefinition)
        {
            return new SequencePipelineSelectStageBuilder<TInput, TSelect>(accessExpressionDefinition, null, null);
        }

        /// <summary>
        /// Transform the property 'from' used to describe the 'build' parameter to <see cref="AccessExpressionDefinition"/>
        /// </summary>
        private async ValueTask<AccessExpressionDefinition?> CompileAccessFrom<TInput>(string? type,
                                                                                       SelectionModeEnum mode,
                                                                                       IReadOnlyCollection<string> fromInputArgs)
        {
            var parameter = NoneType.IsEqualTo<TInput>()
                        ? null
                        : Expression.Parameter(typeof(TInput), "input");

            var inputParameters = parameter?.AsEnumerable().ToArray() ?? EnumerableHelper<ParameterExpression>.ReadOnlyArray;
            var args = CompileArgs<TInput>(fromInputArgs, inputParameters);

            // TODO : Managed arg ref resolution

            if (mode == SelectionModeEnum.None)
            {
                if (!string.IsNullOrEmpty(type))
                    mode = SelectionModeEnum.Build;
                else
                    mode = SelectionModeEnum.Property;
            }

            LambdaExpression? accesExpression = null;
            if (mode == SelectionModeEnum.Build)
            {
                if (string.IsNullOrEmpty(type))
                    throw new InvalidOperationException("Parameter Type must be set in build mode");

                var outputType = GetRefId(type, RefTypeEnum.Type);
                var output = await this._solverService.GetReferenceTypeAsync(outputType);

                if (output?.Item1 == null)
                    throw new KeyNotFoundException("Type not found " + outputType);

                var candidateCtors = output.Item1.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                                                 .Where(c => c.GetParameters().Length == args.Count)
                                                 .ToArray();

                // Prefer an exact type match, then fallback on compatible (assignable or numeric convertible) parameters
                var ctor = candidateCtors.FirstOrDefault(c => c.GetParameters().Select((p, i) => p.ParameterType == args[i].type).All(v => v))
                        ?? candidateCtors.FirstOrDefault(c => c.GetParameters().Select((p, i) => ParameterMatch(p, args[i])).All(v => v));

                if (ctor is null)
                    throw new InvalidDataException("Couldn't found a constructor compatible with args " + string.Join(", ", args.Select(a => a.type)));

                var ctorParameters = ctor.GetParameters();
                var ctorArgs = args.Select((a, i) => a.Value.Type == ctorParameters[i].ParameterType
                                                            ? a.Value
                                                            : Expression.Convert(a.Value, ctorParameters[i].ParameterType))
                                   .ToArray();

                accesExpression = Expression.Lambda(Expression.New(ctor, ctorArgs), inputParameters);
            }
            else if (mode == SelectionModeEnum.Property)
            {
                if (args.Count > 1)
                    throw new InvalidOperationException("Does accept multiple parameter on mode Property : " + type);

                accesExpression = Expression.Lambda(args.First().Value, inputParameters);
            }

            if (accesExpression is not null && accesExpression.Body is ConstantExpression cts)
            {
                return new AccessExpressionDefinition((ConcretBaseType)cts.Type.GetAbstractType(),
                                                      TypedArgument.From(cts.Value, cts.Type),
                                                      null,
                                                      null);
            }

            return accesExpression?.CreateAccess();
        }

        #endregion

        #endregion

        #endregion
    }
}
