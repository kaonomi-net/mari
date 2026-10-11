// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.UnitTests.ThreadExecutors
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Diagnostics;
    using MARI.Framework.Core.Abstractions.Repositories;
    using MARI.Framework.Core.Abstractions.Sequence;
    using MARI.Framework.Core.Abstractions.Sequence.Stages;
    using MARI.Framework.Node.Abstractions;
    using MARI.Framework.Node.Abstractions.Models;
    using MARI.Framework.Node.ThreadExecutors;
    using MARI.Toolbox.Abstractions.Disposables;
    using MARI.Toolbox.Abstractions.Services;
    using MARI.Toolbox.Models;
    using MARI.UnitTests.ToolKit;
    using MARI.UnitTests.ToolKit.VGrains.Transformers;

    using NFluent;

    using NSubstitute;

    using System;
    using System.Diagnostics;
    using MARI.Framework.Core;

    /// <summary>
    /// Test <see cref="SequenceExecutorThreadStageForeach"/> responsable to call vgrain method during flow resolution
    /// </summary>
    public sealed class SequenceExecutorForeachThreadStageProviderUnitTests
    {
        /// <summary>
        /// Test <see cref="SequenceExecutorThreadStageForeach.ExecAsync"/> with valid value
        /// </summary>
        [Fact]
        public async Task SequenceExecutorForeachThreadStageProvider_ExecAsync()
        {
            var rand = new Random();

            // Prepare
            var foreachExecutor = new SequenceExecutorThreadStageForeach();

            var def = typeof(ITestExtractEmailTransformer).GetMethod(nameof(ITestExtractEmailTransformer.ExtractEmailsAsync))!.GetAbstractMethod();

            var callDefinition = new SequenceStageCallDefinition(Guid.NewGuid(),
                                                                 "Root",
                                                                 typeof(string).GetAbstractType(),
                                                                 (ConcretType)typeof(ITestExtractEmailTransformer).GetAbstractType(),
                                                                 def,
                                                                 typeof(string[]).GetAbstractType(),
                                                                 null,
                                                                 null,
                                                                 null,
                                                                 null);

            var innerDef = new SequenceDefinition(Guid.NewGuid(),
                                                  RefIdHelper.Generate(Core.Abstractions.Enums.RefTypeEnum.Sequence, "sequence-test-foreach", "unit.tests"),
                                                  "test",
                                                  SequenceOptionDefinition.Default,
                                                  new[] { callDefinition },
                                                  null);

            var foreachDefinition = new SequenceStageForeachDefinition(Guid.NewGuid(),
                                                                       "Foreach Test",
                                                                       typeof(string[]).GetAbstractType(),
                                                                       innerDef,
                                                                       typeof(string).GetAbstractType(),
                                                                       typeof(string[]).GetAbstractType(),
                                                                       null,
                                                                       null,
                                                                       null);

            var execContext = new MARI.Framework.Core.Models.ExecutionContext(Guid.NewGuid(), Guid.NewGuid(), null);
            var logger = new MemoryTestLogger();

            var inputs = Enumerable.Range(0, rand.Next(5, 15))
                                   .Select(_ => Guid.NewGuid().ToString())
                                   .ToArray();

            var resultInputs = inputs.ToDictionary(k => k, _ => Guid.NewGuid().ToString());

            IReadOnlyCollection<ISequenceExecutorExecThread>? testInnerThreads = null;
            var testMockInnerThreads = new List<ISequenceExecutorExecThread>();

            var diagnosticLogs = new List<IExecutionContextChangeDiagnosticLog>();

            var mockDiagnositicLogger = Substitute.For<IDiagnosticLogger>();
            var mockVGrain = Substitute.For<ITestExtractEmailTransformer>();
            var mockVGrainProvider = Substitute.For<IVGrainProvider>();
            var mariSerializerMock = Substitute.For<IMARISerializer>();
            var timeManagerMock = Substitute.For<ITimeManager>();

            var mockSecureToken = Substitute.For<ISecureContextToken<ISequenceExecutorThreadHandler>>();
            var mockThreadHandler = Substitute.For<ISequenceExecutorThreadHandler>();

            mockSecureToken.Token.Returns(mockThreadHandler);

            Func<SequenceStageDefinition, Func<ISecureContextToken<ISequenceExecutorThreadHandler>>, Task<StageStepResult>>? postCallback = null;

            // Diagnositic Logger

            mockDiagnositicLogger.When(m => m.Log(Arg.Any<IExecutionContextChangeDiagnosticLog>()))
                                 .Do(ci => diagnosticLogs.Add(ci.ArgAt<IExecutionContextChangeDiagnosticLog>(0)));

            // Secure mockInnerThread Tken Mock
            mockThreadHandler.When(m => m.RegisterPostProcess(Arg.Any<Func<SequenceStageDefinition, Func<ISecureContextToken<ISequenceExecutorThreadHandler>>, Task<StageStepResult>>>()))
                             .Do(ci => postCallback = ci.ArgAt<Func<SequenceStageDefinition, Func<ISecureContextToken<ISequenceExecutorThreadHandler>>, Task<StageStepResult>>>(0));

            mockThreadHandler.CreateInnerThread(Arg.Any<SequenceExecutorExecThreadState>(), innerDef, Arg.Any<IExecutionContext>())
                             .Returns(ci =>
                             {
                                 var state = ci.ArgAt<SequenceExecutorExecThreadState>(0);

                                 var mockInnerThread = Substitute.For<ISequenceExecutorExecThread>();
                                 var innerCtx = new MARI.Framework.Core.Models.ExecutionContext(state.FlowUid, state.CurrentStageExecId, state.ParentStageExecId);

                                 mockInnerThread.ExecutionContext.Returns(innerCtx);

                                 testMockInnerThreads.Add(mockInnerThread);

                                 return mockInnerThread;
                             });

            mockThreadHandler.HasInnerThreads.Returns(false);

            mockThreadHandler.When(m => m.SetInnerThreads(Arg.Any<IReadOnlyCollection<ISequenceExecutorExecThread>>()))
                             .Do(ci =>
                             {
                                 var threads = ci.ArgAt<IReadOnlyCollection<ISequenceExecutorExecThread>>(0);
                                 Check.That(threads).IsNotNull().And.CountIs(inputs.Length);
                                 testInnerThreads = threads;
                             });

            var state = new SequenceExecutorExecThreadState(execContext.FlowUID,
                                                            Guid.NewGuid(),
                                                            execContext.CurrentExecutionId,
                                                            execContext.ParentExecutionId,
                                                            null,
                                                            inputs);

            mockThreadHandler.GetCurrentDoneThreadState().Returns(state);

            // Act
            //var foreachExecutor = foreachExecutor.Provide(foreachDefinition);

            Check.That(foreachExecutor).IsNotNull().And.IsSameReferenceAs(foreachExecutor);

            var result = await foreachExecutor.ExecAsync(foreachDefinition,
                                                         inputs,
                                                         execContext,
                                                         logger,
                                                         mockDiagnositicLogger,
                                                         mockVGrainProvider,
                                                         () => mockSecureToken);

            Check.That(result.expectedResultType).IsNull();
            Check.That(result.result).IsNotNull().And.WhichMember(r => r.IsCompleted).Verifies(c => c.IsTrue());

            await result.result;

            // Check

            mockThreadHandler.Received(1).RegisterPostProcess(Arg.Any<Func<SequenceStageDefinition, Func<ISecureContextToken<ISequenceExecutorThreadHandler>>, Task<StageStepResult>>>());
            mockThreadHandler.Received(inputs.Length).CreateInnerThread(Arg.Any<SequenceExecutorExecThreadState>(), innerDef, Arg.Any<IExecutionContext>());
            mockThreadHandler.Received(1).SetInnerThreads(Arg.Any<IReadOnlyCollection<ISequenceExecutorExecThread>>());

            Check.That(testInnerThreads).IsNotNull().And.CountIs(inputs.Length);

            mockThreadHandler.ClearReceivedCalls();

            Check.That(postCallback).IsNotNull();

            // Change Test Values

            mockThreadHandler.HasInnerThreads.Returns(true);
            mockThreadHandler.AllInnerThreadsJobDone.Returns(true);

            Debug.Assert(testInnerThreads != null);
            mockThreadHandler.PullInnerThreads(true).Returns(testInnerThreads);

            foreach (var inner in testMockInnerThreads)
            {
                inner.GetSecurityThreadHandler()
                     .Returns(_ =>
                     {
                         var innerMockSecureToken = Substitute.For<ISecureContextToken<ISequenceExecutorThreadHandler>>();
                         var innerThreadHandler = Substitute.For<ISequenceExecutorThreadHandler>();

                         innerMockSecureToken.Token.Returns(innerThreadHandler);

                         var innerState = new SequenceExecutorExecThreadState(inner.ExecutionContext.FlowUID,
                                                                              Guid.NewGuid(),
                                                                              inner.ExecutionContext.CurrentExecutionId,
                                                                              inner.ExecutionContext.ParentExecutionId,
                                                                              null,
                                                                              null);

                         innerState.SetJobIsDone(new[] { Guid.NewGuid().ToString() });

                         innerThreadHandler.GetCurrentDoneThreadState()
                                           .Returns(innerState);

                         return innerMockSecureToken;
                     });
            }

            // Act post process

            var postbackResults = await postCallback!(foreachDefinition, () => mockSecureToken);

            // Check
            Check.That(postbackResults.expectedResultType).IsNotNull().And.IsEqualTo(typeof(string[][]));
            Check.That(postbackResults.result).IsNotNull().And.IsInstanceOfType(typeof(Task<string[][]>));

            var tsk = (Task<string[][]>)postbackResults.result;

            var loopResult = await tsk;

            Check.That(loopResult).IsNotNull()
                                  .And
                                  .CountIs(inputs.Length)
                                  .And
                                  .ContainsOnlyElementsThatMatch(r => r.Length == 1 &&
                                                                      !string.IsNullOrEmpty(r.First()) &&
                                                                      Guid.TryParse(r.First(), out _));

            // Strict behavior: only expected calls are allowed
            Check.That(mockDiagnositicLogger.ReceivedCalls()).CountIs(diagnosticLogs.Count);
            Check.That(mockVGrainProvider.ReceivedCalls()).IsEmpty();
        }
    }
}
