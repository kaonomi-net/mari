// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.UnitTests.ToolKit.Remoting
{
    using MARI.Toolbox.Abstractions.Extensions.Types;
    using MARI.Toolbox.Abstractions.Models;
    using MARI.UnitTests.ToolKit.Extensions;
    using MARI.UnitTests.ToolKit.Models;

    using H.Formatters;
    using H.Pipes;

    using Newtonsoft.Json;

    using NSubstitute;
    using NSubstitute.Core;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Substitute call handler that redirect all <typeparamref name="TService"/> calls to a remote service
    /// </summary>
    internal sealed class DynamicRemotingSiloService<TService> : ICallHandler
        where TService : class
    {
        #region Fields

        private static readonly TimeSpan s_callTimeout;

        private readonly Dictionary<Guid, TaskCompletionSource<object?>> _callRegistry;
        private readonly HashSet<MethodInfo> _mappedMethods;
        private readonly string _uniqueInstanceId;
        private readonly SemaphoreSlim _locker;

        private PipeClient<RemoteCallMessage>? _client;

        #endregion

        #region Ctor

        static DynamicRemotingSiloService()
        {
            s_callTimeout = Debugger.IsAttached ? TimeSpan.FromMinutes(10) : TimeSpan.FromSeconds(10);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicRemotingSiloService"/> class.
        /// </summary>
        public DynamicRemotingSiloService(string uniqueInstanceId)
        {
            this._locker = new SemaphoreSlim(1);
            this._uniqueInstanceId = uniqueInstanceId.Replace(nameof(IRemoteControllerService) + "_", "");
            this._callRegistry = new Dictionary<Guid, TaskCompletionSource<object?>>();
            this._mappedMethods = new HashSet<MethodInfo>();

            this.Object = Substitute.For<TService>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the substitute instance that redirect calls to the remote service
        /// </summary>
        public TService Object { get; }

        #endregion

        #region Methods

        /// <summary>
        ///
        /// </summary>
        public void Map()
        {
            MapRec(typeof(TService), new HashSet<Type>());

            SubstitutionContext.Current.GetCallRouterFor(this.Object)
                                       .RegisterCustomCallHandlerFactory(_ => this);
        }

        /// <inheritdoc />
        public RouteAction Handle(ICall call)
        {
            var mthd = call.GetMethodInfo();

            if (!this._mappedMethods.Contains(mthd.IsGenericMethod ? mthd.GetGenericMethodDefinition() : mthd))
                return RouteAction.Continue();

            if (mthd.ReturnType == typeof(void))
            {
                SolverVoidMethod(call);
                return RouteAction.Return(null);
            }

            return RouteAction.Return(SolverVoidMethodWithReturn(call));
        }

        /// <summary>
        /// Connect to remove servoce
        /// </summary>
        private void EnsureConnectToRemote()
        {
            EnsureConnectToRemoteAsync(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
        }

        /// <summary>
        /// Connect to remove servoce
        /// </summary>
        private async Task EnsureConnectToRemoteAsync(CancellationToken token = default)
        {
            try
            {
                // Acquire the lock BEFORE the try/finally that releases it
                await this._locker.WaitAsync(token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                if (this._client?.IsConnected == true)
                    return;

                if (this._client == null)
                {
                    this._client = new PipeClient<RemoteCallMessage>(this._uniqueInstanceId, reconnectionInterval: TimeSpan.FromMilliseconds(100), formatter: new NewtonsoftJsonFormatter());
                    this._client.MessageReceived += ResponseReceived;
                }

                this._client.AutoReconnect = true;

                Debug.WriteLine("Try to connect on pipe " + this._uniqueInstanceId);
                await this._client.ConnectAsync(token);
                Debug.WriteLine("Connected on pipe " + this._uniqueInstanceId);

            }
            catch (Exception)
            {
            }
            finally
            {
                this._locker.Release();
            }
        }

        /// <summary>
        /// Send as message response
        /// </summary>
        private void ResponseReceived(object? sender, H.Pipes.Args.ConnectionMessageEventArgs<RemoteCallMessage?> e)
        {
            try
            {
                this._locker.Wait();

                Debug.Assert(e.Message != null);
                if (this._callRegistry.TryGetValue(e.Message.ExecutionId, out var task))
                {
                    if (e.Message.IsCancelled)
                    {
                        task.TrySetCanceled();
                        return;
                    }

                    if (!string.IsNullOrEmpty(e.Message.Error))
                    {
                        task.TrySetException(new Exception(e.Message.Error));
                        return;
                    }

                    object? result = null;

                    if (!string.IsNullOrEmpty(e.Message.JsonContent))
                    {
                        result = JsonConvert.DeserializeObject(e.Message.JsonContent, TestRemotingService.SerializationSettings);
                    }

                    task.TrySetResult(result);
                }
            }
            finally
            {
                this._locker.Release();
            }
        }

        /// <summary>
        /// Remote call without expected result
        /// </summary>
        private void SolverVoidMethod(ICall invocation)
        {
            Task.Run(async () => await SolverVoidMethodAsync(invocation)).GetAwaiter().GetResult();
            return;
        }

        /// <summary>
        /// Remote call with result
        /// </summary>
        private object? SolverVoidMethodWithReturn(ICall invocation)
        {
            var result = Task.Run(async () => await SolverVoidMethodAsync(invocation)).GetAwaiter().GetResult();
            return result;
        }

        /// <summary>
        /// Async remote call with result
        /// </summary>
        private async Task<object?> SolverVoidMethodAsync(ICall invocation)
        {
            var invocationMethod = invocation.GetMethodInfo();

            using (var timeoutCallToken = new CancellationTokenSource(s_callTimeout))
            {
                await EnsureConnectToRemoteAsync(timeoutCallToken.Token);

                ArgumentNullException.ThrowIfNull(this._client);

                var taskCompletion = new TaskCompletionSource<object?>();
                var execId = Guid.NewGuid();

                // Acquire the lock BEFORE the try : a cancelled/timed out wait must not release a permit it never acquired
                await this._locker.WaitAsync(timeoutCallToken.Token);
                try
                {
                    this._callRegistry.Add(execId, taskCompletion);
                }
                finally
                {
                    this._locker.Release();
                }

                try
                {
                    var callSerialized = JsonConvert.SerializeObject(new MethodCallMessage(invocationMethod.Name,

                                                                                           // raisedErrorIfDuplicate: true => Prevent duplicate because message sender came from another AppDomain
                                                                                           (invocationMethod.IsGenericMethod
                                                                                                    ? invocationMethod.GetGenericMethodDefinition()
                                                                                                    : invocationMethod).GetUniqueId(raisedErrorIfDuplicate: true),

                                                                                           TypedArgument.From(invocation.GetArguments(),
                                                                                                              invocationMethod.GetParameters().Select(s => s.ParameterType).ToArray()),
                                                                                           invocationMethod.ReflectedType?.AssemblyQualifiedName),

                                                                     TestRemotingService.SerializationSettings);

                    var callMsg = new RemoteCallMessage(this._uniqueInstanceId, execId, callSerialized, string.Empty, false);

                    await this._client.WriteAsync(callMsg, timeoutCallToken.Token);
                    timeoutCallToken.Token.ThrowIfCancellationRequested();

                    // Bounded wait : a lost response must fail the call after s_callTimeout instead of hanging forever
                    await taskCompletion.Task.WaitAsync(timeoutCallToken.Token);

                    var returnType = invocationMethod.ReturnType;

                    var noReturn = returnType != null &&

                                   (returnType == typeof(void) ||
                                    returnType == typeof(Task) ||
                                    returnType == typeof(ValueTask));

                    // Convert result to well format
                    var resultInst = taskCompletion.Task.Result;

                    if (returnType != null && (resultInst?.GetType().IsAssignableTo(returnType) ?? false) == false)
                    {
                        var extendInfo = returnType.GetTypeInfoExtension();

                        if (extendInfo.IsTask || extendInfo.IsValueTask)
                        {
                            if (extendInfo.IsTask)
                            {
                                if (noReturn)
                                    resultInst = Task.CompletedTask;
                                else
                                    resultInst = extendInfo.GetSpecifcTypeExtend<ITaskTypeInfoEnhancer>().GetTaskFromResult(resultInst);
                            }
                            else if (extendInfo.IsValueTask)
                            {
                                if (noReturn)
                                    resultInst = ValueTask.CompletedTask;
                                else
                                    resultInst = extendInfo.GetSpecifcTypeExtend<IValueTaskTypeInfoEnhancer>().GetValueTaskFromResult(resultInst);
                            }
                        }
                        else
                        {
                            throw new InvalidCastException("[Remote][" + invocationMethod + "] Could not convert remote response type " + resultInst!.GetType() + " to method return type " + returnType);
                        }
                    }

                    return resultInst;
                }
                finally
                {
                    // Always unregister the pending call (success, error, timeout)
                    await this._locker.WaitAsync();
                    try
                    {
                        this._callRegistry.Remove(execId);
                    }
                    finally
                    {
                        this._locker.Release();
                    }
                }
            }
        }

        /// <summary>
        /// Recursive map all public mehotd
        /// </summary>
        private void MapRec(Type type, HashSet<Type> mapTypes)
        {
            if (mapTypes.Contains(type))
                return;

            mapTypes.Add(type);

            foreach (var mthd in type.GetMethods())
            {
                // Generic methods are redirected for any generic arguments
                this._mappedMethods.Add(mthd.IsGenericMethod ? mthd.GetGenericMethodDefinition() : mthd);
            }

            if (type.BaseType != null && type.BaseType != typeof(object))
            {
                MapRec(type, mapTypes);
            }

            var interfaces = type.GetInterfaces();
            foreach (var i in interfaces)
            {
                MapRec(i, mapTypes);
            }
        }

        #endregion
    }

    internal static class DyncamicRemotingSiloServiceExtension
    {
        /// <summary>
        /// Creates dynamic remote handler and return the substitute instance that redirect calls to remote service
        /// </summary>
        public static object? CreateRemoteServiceFrom(this Type service, string uniqueInstanceId)
        {
            var finalType = typeof(DynamicRemotingSiloService<>).MakeGenericType(service);

            var mockController = Activator.CreateInstance(finalType, new object[] { uniqueInstanceId });

            ArgumentNullException.ThrowIfNull(mockController);

            var initMthd = mockController.GetType().GetMethod(nameof(DynamicRemotingSiloService<string>.Map));

            ArgumentNullException.ThrowIfNull(initMthd);
            initMthd.Invoke(mockController, Array.Empty<object>());

            var objProp = mockController.GetType().GetProperty(nameof(DynamicRemotingSiloService<string>.Object));

            ArgumentNullException.ThrowIfNull(objProp);
            return objProp.GetValue(mockController);
        }
    }
}
