// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.References
{
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Enums;
    using MARI.Framework.Core.Abstractions.References;
    using MARI.Framework.Core.Executions;

    using MARI.Toolbox;

    using MessagePack;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using Orleans;

    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Relay grain incharge to resolve node way the reference to called
    /// </summary>
    /// <seealso cref="VGrainBase{IMARIReferenceHandlerVGrain}" />
    /// <seealso cref="IMARIReferenceHandlerVGrain" />
    internal sealed class MARIReferenceHandlerVGrain : VGrainBase<IMARIReferenceHandlerVGrain>, IMARIReferenceHandlerVGrain
    {
        #region Fields
        private static readonly MethodInfo s_prepareVGrainLauncher;

        private readonly IMARIReferenceSolverService _referenceSolverService;
        private readonly IMARIExecutionHandler _executionHandler;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="MARIReferenceHandlerVGrain"/> class.
        /// </summary>
        static MARIReferenceHandlerVGrain()
        {
            Expression<Func<MARIReferenceHandlerVGrain, ValueTask<IExecutionLauncher>>> directExpr = e => e.PrepareVGrainLauncherAsync<IVGrain, int, int, NoneType>(default, null!);
            s_prepareVGrainLauncher = ((MethodCallExpression)directExpr.Body).Method.GetGenericMethodDefinition();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MARIReferenceHandlerVGrain"/> class.
        /// </summary>
        public MARIReferenceHandlerVGrain(ILogger<IMARIReferenceHandlerVGrain> logger,
                                               IMARIReferenceSolverService referenceSolverService,
                                               IMARIExecutionHandler executionHandler)
            : base(logger)
        {
            this._referenceSolverService = referenceSolverService;
            this._executionHandler = executionHandler;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task Fire<TRefExecCommand>(TRefExecCommand execCommand)
            where TRefExecCommand : IRefExecuteCommand
        {
            var launcher = await PrepareLauncherAsync<TRefExecCommand, NoneType>(execCommand);

            if (launcher is ICommonExecutionLauncher<IExecutionDirectLauncher> direct)
            {
                await direct.Fire();
                return;
            }

            if (launcher is ICommonExecutionLauncher<IExecutionFlowLauncher> flow)
            {
                await flow.Fire();
                return;
            }

            throw new NotSupportedException("Launcher type " + launcher.GetType() + " are not managed to fire through RefId.");
        }

        /// <inheritdoc />
        public async Task<Guid> FireWithDeferred<TResult, TRefExecCommand>(TRefExecCommand execCommand)
            where TRefExecCommand : IRefExecuteCommand
        {
            var launcher = await PrepareLauncherAsync<TRefExecCommand, TResult>(execCommand);

            if (launcher is IExecutionFlowLauncher flow)
                return await flow.FireWithDeferred<TResult>();

            throw new NotSupportedException("Launcher type " + launcher.GetType() + " are not managed to fire through FireWithDeferred.");
        }

        /// <inheritdoc />
        public async Task<IExecutionResult> RunAsync<TRefExecCommand>(TRefExecCommand execCommand, GrainCancellationToken token)
            where TRefExecCommand : IRefExecuteCommand
        {
            var launcher = await PrepareLauncherAsync<TRefExecCommand, NoneType>(execCommand);
            return await launcher.RunAsync(token.CancellationToken);
        }

        /// <inheritdoc />
        public async Task<IExecutionResult<TExpectedOutput>> RunAsync<TExpectedOutput, TRefExecCommand>(TRefExecCommand execCommand, GrainCancellationToken token)
            where TRefExecCommand : IRefExecuteCommand
        {
            var launcher = await PrepareLauncherAsync<TRefExecCommand, TExpectedOutput>(execCommand);
            return await launcher.RunAsync<TExpectedOutput>(token.CancellationToken);
        }

        /// <inheritdoc />
        public async Task<IExecutionResult> RunWithAnyResultAsync<TRefExecCommand>(TRefExecCommand execCommand, GrainCancellationToken token) where TRefExecCommand : IRefExecuteCommand
        {
            var launcher = await PrepareLauncherAsync<TRefExecCommand, AnyType>(execCommand);
            return await launcher.RunWithAnyResultAsync(token.CancellationToken);
        }

        #region Tools

        /// <summary>
        /// Prepares the launcher based on the command
        /// </summary>
        private async ValueTask<IExecutionLauncher> PrepareLauncherAsync<TRefExecCommand, TExpectResult>(TRefExecCommand execCommand)
            where TRefExecCommand : IRefExecuteCommand
        {
            ArgumentNullException.ThrowIfNull(execCommand);

            var mainTarget = execCommand.ExecReference;

            // TODO : Unit Test
            if (RefIdHelper.IsRefId(mainTarget) == false)
                throw new InvalidDataException("Main reference must be a correct formated RefId look for RefIdHelper : Invalid Uri (" + mainTarget + ")");

            var resultTraits = typeof(TExpectResult);

            IExecutionLauncher? launcher = null;
            var targetType = RefIdHelper.GetDefinitionType(mainTarget);

            if (targetType == RefTypeEnum.VGrain)
            {
                var vgrainType = await this._referenceSolverService.GetReferenceTypeAsync(execCommand.ExecReference);

                if (vgrainType is null)
                    throw new KeyNotFoundException("Reference {0} don't have been founded".WithArguments(execCommand.ExecReference));

                var methodCallGenericTypes = new[]
                {
                    vgrainType.Item1,
                    execCommand.GetInputType() ?? NoneType.Trait,
                    execCommand.GetConfigType() ?? NoneType.Trait,
                    resultTraits
                };

                launcher = await ((ValueTask<IExecutionLauncher>)s_prepareVGrainLauncher.MakeGenericMethod(methodCallGenericTypes).Invoke(this, new object?[] { execCommand, vgrainType.Item2 })!);
            }

            if (launcher is null)
                throw new InvalidOperationException("Could not format launcher associate to command type : " + execCommand.GetType() + " with main reference target " + mainTarget);

            return launcher;
        }

        /// <summary>
        /// Prepares the vgrain launcher.
        /// </summary>
        private async ValueTask<IExecutionLauncher> PrepareVGrainLauncherAsync<TVGrain, TInput, TConfig, TExpectedResult>(RefVGrainExecuteCommand<TInput, TConfig> execCommand, Uri? vgrainRedIf)
            where TVGrain : IVGrain
        {
            ArgumentNullException.ThrowIfNull(execCommand);
            ArgumentNullException.ThrowIfNullOrEmpty(execCommand.simpleMethodNameIdentifier);

            var mthRefId = RefIdHelper.WithMethod(vgrainRedIf ?? execCommand.ExecReference, execCommand.simpleMethodNameIdentifier);
            var method = await this._referenceSolverService.GetReferenceMethodAsync(mthRefId, typeof(TVGrain)); // vgrainRedIf ?? 

            if (method is null)
                throw new KeyNotFoundException("Reference {0} don't have been founded".WithArguments(mthRefId));

            var grainParameter = Expression.Parameter(typeof(TVGrain), "g");
            var inputParameter = (NoneType.IsEqualTo<TInput>() ? null : Expression.Parameter(typeof(TInput), "i"));
            var contextParameter = (NoneType.IsEqualTo<TConfig>()
                                            ? Expression.Parameter(typeof(IExecutionContext), "ctx")
                                            : Expression.Parameter(typeof(IExecutionContext<TConfig>), "ctx"));

            ParameterExpression[] allParameters;

            if (inputParameter == null || method.GetParameters().Length == 1)
                allParameters = new[] { grainParameter, contextParameter };
            else
                allParameters = new[] { grainParameter, inputParameter, contextParameter };

            var bodyCallMethodExpression = Expression.Call(grainParameter, method, allParameters.Skip(1).ToArray());
            var callExpression = Expression.Lambda(bodyCallMethodExpression, allParameters);

            var launcher = new ExecutionDirectLauncher<TVGrain, TInput, TConfig, TExpectedResult>(this.ServiceProvider.GetService<ILogger<IMARIExecutionHandler>>() ?? NullLogger<IMARIExecutionHandler>.Instance,
                                                                                                  this.ServiceProvider.GetRequiredService<IVGrainProvider>(),
                                                                                                  execCommand.Config,
                                                                                                  execCommand.Input,
                                                                                                  callExpression,
                                                                                                  execCommand.ForceId);

            return (IExecutionLauncher)launcher;
        }

        #endregion

        #endregion
    }
}
