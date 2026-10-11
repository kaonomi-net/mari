// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.CodeGenerator.UnitTests
{
    using Castle.Core.Internal;

    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Attributes.MetaData;
    using MARI.Framework.Core.Abstractions.Enums;
    using MARI.Framework.Core.Abstractions.Models;
    using MARI.Framework.Core.Abstractions.References;
    using MARI.Framework.Core.Abstractions.Signals;
    using MARI.Framework.Core.Abstractions.Streams;
    using MARI.Framework.Core.CodeGenerator.UnitTests.Models;
    using MARI.Framework.Node.References;

    using MARI.Toolbox.Abstractions.Comparers;
    using MARI.Toolbox.Extensions;
    using MARI.Toolbox.Loggers;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using NFluent;

    using NSubstitute;
    using NSubstitute.Core;
    using NSubstitute.ExceptionExtensions;

    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Xunit;

    /// <summary>
    /// Test for <see cref="MARIReferenceSolverService"/>
    /// </summary>
    public sealed class MARIReferenceSolverServiceUTest
    {
        #region Fields

        private static IReadOnlyCollection<ReferenceTarget>? s_references;

        private static readonly MARIReferenceProviderAttribute? s_attr;
        private static readonly Assembly s_current;

        private static readonly IMARITypeReferenceGrainServiceClient s_typeClusterSolver;
        private static readonly IDefinitionProvider s_definitionProvider;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="MARIReferenceSolverServiceUTest"/> class.
        /// </summary>
        static MARIReferenceSolverServiceUTest()
        {
            s_current = typeof(CodeGenUTest).Assembly;
            s_attr = s_current.GetCustomAttributes().OfType<MARIReferenceProviderAttribute>().SingleOrDefault();

            // empty mock
            s_typeClusterSolver = Substitute.For<IMARITypeReferenceGrainServiceClient>();
            s_definitionProvider = Substitute.For<IDefinitionProvider>();
        }

        #endregion

        #region Methods

        [Theory]
        [MemberData(nameof(GetTestReferences))]

        public async Task Solver_Type_And_Method(string uriStr)
        {
            Check.That(uriStr).IsNotNull().And.IsNotEmpty();

            var uri = new Uri(uriStr);
            var refs = GetReferenceTargets();

            var typeRefGrainServiceMock = Substitute.For<IMARITypeReferenceGrainServiceClient>();
            typeRefGrainServiceMock.GetLatestRegistryAsync(Arg.Any<string>(), Arg.Any<GrainCancellationToken>()).Returns(new ReferenceTargetRegistry("azeaz", refs));

            using (var solver = new MARIReferenceSolverService(typeRefGrainServiceMock, s_definitionProvider, NullLogger<IMARIReferenceSolverService>.Instance))
            {
                var type = RefIdHelper.GetDefinitionType(uri);

                Uri? solveUri = null;
                if (type == Abstractions.Enums.RefTypeEnum.Method)
                {
                    var mthod = await solver.GetReferenceMethodAsync(uri);

                    Check.That(mthod).IsNotNull();
                    solveUri = RegenUriRefFromMethod(mthod);
                }
                else
                {
                    var solvedType = await solver.GetReferenceTypeAsync(uri);

                    Check.That(solvedType).IsNotNull();
                    Check.That(solvedType!.Item1).IsNotNull();
                    solveUri = RegenUriRefFromType(solvedType!.Item1);
                }

                Check.That(solveUri.ToString()).IsNotNull()
                                               .And
                                               .IsEqualTo(uri.ToString());
            }
        }

        [Theory]
        [MemberData(nameof(GetMethodSNI), parameters: typeof(ISimpleWithMethodGrain))]
        [MemberData(nameof(GetMethodSNI), parameters: typeof(IGenericSimpleWithMethodGrain<>))]
        public async Task Solver_Inherite_Method(string inheritMethod)
        {
            var childType = RegenUriRefFromType(typeof(IInheriteWithMethodGrain));

            var childWithParentMethod = RefIdHelper.WithMethod(childType, inheritMethod);

            var refs = GetReferenceTargets();

            var typeRefGrainServiceMock = Substitute.For<IMARITypeReferenceGrainServiceClient>();
            typeRefGrainServiceMock.GetLatestRegistryAsync(Arg.Any<string>(), Arg.Any<GrainCancellationToken>()).Returns(new ReferenceTargetRegistry("azeaz", refs));

            using (var solver = new MARIReferenceSolverService(typeRefGrainServiceMock, s_definitionProvider, NullLogger<IMARIReferenceSolverService>.Instance))
            {
                var method = await solver.GetReferenceMethodAsync(childWithParentMethod);

                var regenMethodUri = RegenUriRefFromMethod(method);

                var sourceMethodName = RefIdHelper.GetMethodName(childWithParentMethod);
                var regenMethodName = RefIdHelper.GetMethodName(regenMethodUri);

                Check.That(sourceMethodName).IsEqualTo(regenMethodName);

                var sourceType = await solver.GetReferenceTypeAsync(childType);
                var declarationType = await solver.GetReferenceTypeAsync(regenMethodUri);

                Check.That(sourceType?.Item1).IsNotNull();
                Check.That(declarationType?.Item1).IsNotNull();

                Check.That(sourceType!.Item1).IsNotEqualTo(declarationType?.Item1);

                if (sourceType.Item1.IsGenericType == false && declarationType!.Item1.IsGenericType == false)
                    Check.ThatCode(() => sourceType!.Item1.IsAssignableTo(declarationType!.Item1)).DoesNotThrow().And.WhichResult().IsTrue();
            }
        }

        [Fact]
        public async Task Solver_Definition_uid()
        {
            var sequencesProvider = Substitute.For<IDefinitionProvider>();

            var typed = new RefTypeEnum[]
            {
                RefTypeEnum.Door,
                RefTypeEnum.Sequence,
                RefTypeEnum.Signal,
                RefTypeEnum.Trigger,
                RefTypeEnum.StreamQueue
            };

            var ns = string.Join(".", Guid.NewGuid()
                              .ToString()
                              .Split('-')
                              .Take(2));

            var definitions = Enumerable.Range(5, 10)
                                        .Select(x => RefIdHelper.Generate(typed.ElementAt(Random.Shared.Next(0, typed.Length)),
                                                                          string.Join("-", Guid.NewGuid().ToString().Split('-').Take(3)),
                                                                          ns))
                                        .ToDictionary(k => k, v => Guid.NewGuid());

            var definitionsMocks = definitions.Select(d =>
            {
                var mockDef = Substitute.For<IDefinition, IRefDefinition>();
                mockDef.Uid.Returns(d.Value);
                ((IRefDefinition)mockDef).RefId.Returns(d.Key);
                return mockDef;
            }).ToArray();

            sequencesProvider.GetKeysAsync(Arg.Any<Expression<Func<IDefinition, bool>>>(), Arg.Any<CancellationToken>()).Returns((CallInfo info) =>
            {
                var exprFilter = info.Arg<Expression<Func<IDefinition, bool>>>();

                Check.That(exprFilter).IsNotNull();
                var filter = exprFilter.Compile();

                var results = definitionsMocks.Where(filter)
                                              .Select(e => e.Uid)
                                              .ToArray();

                return ValueTask.FromResult<IReadOnlyCollection<Guid>>(results);
            });

            sequencesProvider.GetValuesAsync(Arg.Any<Expression<Func<IDefinition, bool>>>(), Arg.Any<CancellationToken>()).Returns((CallInfo info) =>
            {
                var exprFilter = info.Arg<Expression<Func<IDefinition, bool>>>();

                Check.That(exprFilter).IsNotNull();
                var filter = exprFilter.Compile();

                var results = definitionsMocks.Where(filter)
                                              .ToArray();

                return ValueTask.FromResult<IReadOnlyCollection<IDefinition>>(results);
            });

            using (var solver = new MARIReferenceSolverService(s_typeClusterSolver, sequencesProvider, NullLogger<IMARIReferenceSolverService>.Instance))
            {
                foreach (var def in definitions)
                {
                    var resolvedDef = await solver.GetReferenceDefinitionsAsync(def.Key);
                    var resolvedDefUid = await solver.GetReferenceDefinitionUidAsync(def.Key);

                    Check.That(resolvedDef).IsNotNull().And.CountIs(1);
                    Check.That(resolvedDefUid).IsNotNull().And.CountIs(1);
                    Check.That(resolvedDef.Single().Uid).IsEqualTo(def.Value)
                                                        .And
                                                        .IsEqualTo(resolvedDefUid.Single());

                    RefIdHelper.Explode(def.Key, out var type, out var _, out var sni);

                    var withoutNS = RefIdHelper.Generate(type, sni);

                    var withoutNSResolvedDef = await solver.GetReferenceDefinitionsAsync(withoutNS);
                    var withoutNSResolvedDefUid = await solver.GetReferenceDefinitionUidAsync(withoutNS);

                    Check.That(withoutNSResolvedDef).IsNotNull().And.CountIs(1);
                    Check.That(withoutNSResolvedDefUid).IsNotNull().And.CountIs(1);
                    Check.That(withoutNSResolvedDef.Single().Uid).IsEqualTo(def.Value)
                                                                 .And
                                                                 .IsEqualTo(withoutNSResolvedDefUid.Single());
                }
            }
        }

        #region Tools

        private Uri RegenUriRefFromType(Type solvedType)
        {
            Check.That(solvedType).IsNotNull();

            var metaDataAttr = solvedType.GetAttribute<VGrainMetaDataAttribute>();
            var simpleMetaDataAttr = solvedType.GetAttribute<RefSimpleNameIdentifierAttribute>();

            var refType = RefTypeEnum.Type;
            if (solvedType.IsAssignableTo(typeof(IVGrain)))
                refType = solvedType.IsInterface ? RefTypeEnum.VGrain : RefTypeEnum.VGrainImplementation;

            var sni = metaDataAttr?.SimpleNameIdentifier ?? simpleMetaDataAttr?.SimpleNameIdentifier;
            var ns = metaDataAttr?.NamespaceIdentifier ?? simpleMetaDataAttr?.NamespaceIdentifier;

            Check.That(sni).Not.IsNullOrEmpty();

            return RefIdHelper.Generate(refType, sni!, ns);
        }

        private Uri RegenUriRefFromMethod(MethodInfo? mthod)
        {
            Check.That(mthod).IsNotNull();
            Check.That(mthod!.DeclaringType).IsNotNull();

            var typeUri = RegenUriRefFromType(mthod.DeclaringType!);

            var attr = mthod.GetAttribute<VGrainMetaDataMethodAttribute>();

            Check.That(attr).IsNotNull();
            Check.That(attr.SimpleNameIdentifier).IsNotNull().And.Not.IsNullOrEmpty();

            return RefIdHelper.WithMethod(typeUri, attr.SimpleNameIdentifier);
        }

        /// <summary>
        /// Gets the test references.
        /// </summary>
        public static TheoryData<string> GetTestReferences()
        {
            var refs = GetReferenceTargets();

            var uris = refs.Select(s => s.RefId.ToString())
                           .ToArray();

            return new TheoryData<string>(uris);
        }

        public static TheoryData<string> GetMethodSNI(Type sourceType)
        {
            var methodSNI = sourceType.GetAllMethodInfos()
                                      .Select(m => m.GetCustomAttribute<VGrainMetaDataMethodAttribute>())
                                      .NotNull()
                                      .Select(m => m.SimpleNameIdentifier)
                                      .Distinct()
                                      .ToArray();

            return new TheoryData<string>(methodSNI);
        }

        /// <summary>
        /// Gets the reference targets.
        /// </summary>
        private static IReadOnlyCollection<ReferenceTarget> GetReferenceTargets()
        {
            lock (s_current)
            {
                if (s_references is null)
                {
                    Check.That(s_attr).IsNotNull();

                    var testLogger = new InMemoryLogger(new LoggerFilterOptions().ToMonitorOption());

                    var textCtx = new MARIReferenceRegistry(testLogger);
                    s_attr!.Populate(textCtx);

                    var logs = testLogger.GetLogsCopy();
                    Check.WithCustomMessage(string.Join("\n", logs)).That(logs).IsNullOrEmpty();

                    s_references = textCtx.GetReferences();
                }
            }

            return s_references;

        }

        #endregion

        #endregion
    }
}
