// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Models
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Repositories;

    using MARI.Toolbox.Models;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.Metrics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Threading;

    /// <summary>
    /// Define an execution context
    /// </summary>
    /// <seealso cref="IExecutionContext" />
    [Immutable]
    [Serializable]
    [ImmutableObject(true)]
    public class ExecutionContext : IExecutionContext, IExecutionContextInternal
    {
        #region Fields

        private static readonly MethodInfo s_grainTokenCancelMthd;

        private static readonly Dictionary<Type, FieldInfo> s_grainReferenceRuntimeCache;
        private static readonly ReaderWriterLockSlim s_grainReferenceRuntimeCacheLocker;

        private static readonly Dictionary<Type, MethodInfo> s_specializedGenericMthd;
        private static readonly ReaderWriterLockSlim s_rwMethodCache;
        private static readonly MethodInfo s_duplicateGenericMthd;

        private static readonly MethodInfo s_addGrainCancelReference;
        private static readonly PropertyInfo s_propGrainRuntime;

        // Copy-on-write immutable set : instance may be shared between local grains ([Immutable]) so never mutate the set in place
        private readonly object _contextDataLocker = new object();
        private volatile ImmutableHashSet<IContextDataContainer> _contextDataContainers;

        [NonSerialized]
        private GrainCancellationToken? _grainCancelToken;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ExecutionContext"/> class.
        /// </summary>
        static ExecutionContext()
        {
            s_grainReferenceRuntimeCache = new Dictionary<Type, FieldInfo>();
            s_specializedGenericMthd = new Dictionary<Type, MethodInfo>();

            s_duplicateGenericMthd = typeof(ExecutionContext).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                                             .Single(m => m.Name == nameof(DuplicateWithConfiguration) && m.IsGenericMethodDefinition);

            var propGrainRuntime = typeof(GrainReference).GetProperty("Runtime", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);
            Debug.Assert(propGrainRuntime != null);
            s_propGrainRuntime = propGrainRuntime;

            var addGrainCancelReference = typeof(GrainCancellationToken).GetMethod("AddGrainReference", BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(addGrainCancelReference != null);
            s_addGrainCancelReference = addGrainCancelReference;

            var grainTokenCancel = typeof(GrainCancellationToken).GetMethod("Cancel", BindingFlags.NonPublic | BindingFlags.Instance, Type.EmptyTypes);
            Debug.Assert(grainTokenCancel != null);
            s_grainTokenCancelMthd = grainTokenCancel;

            s_rwMethodCache = new ReaderWriterLockSlim();
            s_grainReferenceRuntimeCacheLocker = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ExecutionContext"/> class from being created.
        /// </summary>
        public ExecutionContext(Guid flowUID,
                                Guid currentExecutionId,
                                Guid? parentExecutionId)
        {
            // IGrainCancellationTokenRuntime
            //this._grainCancelTokenSource = Activator.CreateInstance(typeof(GrainCancellationToken),  new object[] { flowUID, false,  })

            this.FlowUID = flowUID;
            this.CurrentExecutionId = currentExecutionId;
            this.ParentExecutionId = parentExecutionId;

            this._contextDataContainers = ImmutableHashSet<IContextDataContainer>.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionContext"/> class.
        /// </summary>
        /// <remarks>
        ///     Used to create a linked context
        /// </remarks>
        private ExecutionContext(Guid flowUID,
                                 Guid currentExecutionId,
                                 Guid? parentExecutionId,
                                 GrainCancellationToken? grainCancellationToken)
        {
            this._grainCancelToken = grainCancellationToken;
            this.FlowUID = flowUID;
            this.CurrentExecutionId = currentExecutionId;
            this.ParentExecutionId = parentExecutionId;

            this._contextDataContainers = ImmutableHashSet<IContextDataContainer>.Empty;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public Guid FlowUID { get; }

        /// <inheritdoc />
        public Guid CurrentExecutionId { get; }

        /// <inheritdoc />
        [IgnoreDataMember]
        public CancellationToken CancellationToken
        {
            get
            {
                ArgumentNullException.ThrowIfNull(this._grainCancelToken, "IExecutionContextInternal.AddCancelGrainReference(GrainReference grainRef) must be call before any other call");
                return this._grainCancelToken!.CancellationToken;
            }
        }

        /// <inheritdoc />
        public Guid? ParentExecutionId { get; }

        /// <inheritdoc />
        [IgnoreDataMember]
        GrainCancellationToken? IExecutionContextInternal.GrainCancellationToken
        {
            get { return this._grainCancelToken; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ILogger GetLogger<T>(ILoggerFactory loggerFactory)
            where T : IVGrain
        {
            lock (this)
            {
                return GetLogger(loggerFactory, this.FlowUID, this.CurrentExecutionId, typeof(T).Name);
            }
        }

        /// <inheritdoc />
        public ILogger GetLogger(ILoggerFactory loggerFactory, Type type)
        {
            lock (this)
            {
                return GetLogger(loggerFactory, this.FlowUID, this.CurrentExecutionId, type.Name);
            }
        }

        /// <inheritdoc />
        public IExecutionContext NextContext()
        {
            var next = new ExecutionContext(this.FlowUID,
                                            Guid.NewGuid(),
                                            this.CurrentExecutionId,
                                            this._grainCancelToken);

            next.CopyContextImportantInformation(this);
            return next;
        }

        /// <inheritdoc />
        public IExecutionContext<TContextInfo> DuplicateWithConfiguration<TContextInfo>(TContextInfo contextInfo)
        {
            var ctx = new ExecutionContextWithConfiguration<TContextInfo>(this.FlowUID,
                                                                          this.CurrentExecutionId,
                                                                          this.ParentExecutionId,
                                                                          contextInfo);

            ctx.CopyContextImportantInformation(this);
            return ctx;
        }

        /// <inheritdoc />
        public IExecutionContext DuplicateWithConfiguration(object? contextInfo, Type contextType)
        {
            MethodInfo? genericMthd = null;

            s_rwMethodCache.EnterReadLock();
            try
            {
                s_specializedGenericMthd.TryGetValue(contextType, out genericMthd);
            }
            finally
            {
                s_rwMethodCache.ExitReadLock();
            }

            if (genericMthd == null)
            {
                s_rwMethodCache.EnterWriteLock();
                try
                {
                    genericMthd = s_duplicateGenericMthd.MakeGenericMethod(contextType);

                    if (!s_specializedGenericMthd.ContainsKey(contextType))
                        s_specializedGenericMthd.Add(contextType, genericMthd);
                }
                finally
                {
                    s_rwMethodCache.ExitWriteLock();
                }
            }

            return (IExecutionContext)(genericMthd?.Invoke(this, new object?[] { contextInfo }) ?? throw new InvalidOperationException("Invalid context cast"));
        }

        /// <inheritdoc />
        public virtual IExecutionContext Duplicate()
        {
            var duplicate = new ExecutionContext(this.FlowUID, this.CurrentExecutionId, this.ParentExecutionId, this._grainCancelToken);
            duplicate.CopyContextImportantInformation(this);
            return duplicate;
        }

        /// <inheritdoc />
        public TContextData? TryGetContextData<TContextData>(IMARISerializer serializer) where TContextData : struct
        {
            var trait = typeof(TContextData);
            var container = this._contextDataContainers.FirstOrDefault(c => c.IsMatch(trait));

            if (container is null)
                return default;

            return (TContextData?)container.GetData(serializer);
        }

        /// <inheritdoc />
        public object? TryGetContextData(ConcretType type, IMARISerializer serializer)
        {
            var container = this._contextDataContainers.FirstOrDefault(c => c.IsMatch(type));

            if (container is null)
                return default;

            return container.GetData(serializer);
        }

        /// <inheritdoc />
        public bool TryPushContextData<TContextData>(TContextData contextData,
                                                     bool @override,
                                                     IMARISerializer serializer)
            where TContextData : struct
        {
            var trait = typeof(TContextData);

            lock (this._contextDataLocker)
            {
                var containers = this._contextDataContainers;
                var exist = containers.FirstOrDefault(c => c.IsMatch(trait));

                if (exist is not null)
                {
                    if (!@override)
                        return false;

                    containers = containers.Remove(exist);
                }

                var newData = ContextDataContainer<TContextData>.Create(contextData, serializer);
                this._contextDataContainers = containers.Add(newData);
                return true;
            }
        }

        /// <inheritdoc />
        public bool TryPushContextData(IContextDataContainer contextDataContainer, bool @override)
        {
            lock (this._contextDataLocker)
            {
                var containers = this._contextDataContainers;
                var exist = containers.FirstOrDefault(c => c.IsMatch(contextDataContainer));

                if (exist is not null)
                {
                    if (!@override)
                        return false;

                    containers = containers.Remove(exist);
                }

                this._contextDataContainers = containers.Add(contextDataContainer);
                return true;
            }
        }

        /// <inheritdoc />
        public void ClearContextData()
        {
            lock (this._contextDataLocker)
            {
                this._contextDataContainers = ImmutableHashSet<IContextDataContainer>.Empty;
            }
        }

        /// <inheritdoc />
        public void ClearContextData(Type dataType)
        {
            lock (this._contextDataLocker)
            {
                this._contextDataContainers = this._contextDataContainers.Except(this._contextDataContainers.Where(c => c.IsMatch(dataType)));
            }
        }

        /// <inheritdoc />
        public void ClearContextData<TContextData>() where TContextData : struct
        {
            ClearContextData(typeof(TContextData));
        }

        /// <inheritdoc />
        public IReadOnlyCollection<IContextDataContainer> GetAllDataContext()
        {
            return this._contextDataContainers;
        }

        #region Tools

        /// <summary>
        /// Copies the context important information.
        /// </summary>
        public void CopyContextImportantInformation(IExecutionContext source)
        {
            InjectAllDataContext(source.GetAllDataContext());

            if (source is IExecutionContextInternal executionContextInternal)
                this._grainCancelToken = executionContextInternal.GrainCancellationToken;
        }

        /// <inheritdoc />
        internal void InjectAllDataContext(IReadOnlyCollection<IContextDataContainer> contextDataContainers)
        {
            lock (this._contextDataLocker)
            {
                this._contextDataContainers = ImmutableHashSet.CreateRange(contextDataContainers);
            }
        }

        /// <inheritdoc cref="IExecutionContext.GetLogger{T}(ILoggerProvider)" />
        private static ILogger GetLogger(ILoggerFactory loggerFactory, Guid flowUid, Guid currentExecutionId, string category)
        {
            // Use a stable category (LoggerFactory caches one logger per category forever) and prefix messages with flow/execution ids
            return new ExecutionPrefixLogger(loggerFactory.CreateLogger(category),
                                             string.Format("[Flow {0}][VGrain {1} - {2}]", flowUid, currentExecutionId, category));
        }

        /// <summary>
        /// Logger wrapper that prefixes every message with execution information
        /// </summary>
        private sealed class ExecutionPrefixLogger : ILogger
        {
            private readonly ILogger _inner;
            private readonly string _prefix;

            public ExecutionPrefixLogger(ILogger inner, string prefix)
            {
                this._inner = inner;
                this._prefix = prefix;
            }

            /// <inheritdoc />
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            {
                return this._inner.BeginScope(state);
            }

            /// <inheritdoc />
            public bool IsEnabled(LogLevel logLevel)
            {
                return this._inner.IsEnabled(logLevel);
            }

            /// <inheritdoc />
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var prefix = this._prefix;
                this._inner.Log(logLevel, eventId, state, exception, (s, e) => prefix + " " + formatter(s, e));
            }
        }

        /// <inheritdoc />
        public ValueTask Cancel()
        {
            ArgumentNullException.ThrowIfNull(this._grainCancelToken, "IExecutionContextInternal.AddCancelGrainReference(GrainReference grainRef) must be call before any other call");
            //Task.Run(async () => await GrainTokenCancel(this._grainCancelToken));

            // Return the propagation task to let caller await remote grain cancellation and observe failures
            if (s_grainTokenCancelMthd.Invoke(this._grainCancelToken, null) is Task cancelTask)
                return new ValueTask(cancelTask);

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        void IExecutionContextInternal.InitGrainCancelToken(GrainReference grainRef)
        {
            if (this._grainCancelToken is not null)
                return;

            var grainReferenceRuntime = (IGrainReferenceRuntime)s_propGrainRuntime.GetValue(grainRef)!;

            var cancellationTokenRuntime = GetCancellationTokenRuntimeFromCache(grainReferenceRuntime);

            var cancellRuntime = cancellationTokenRuntime.GetValue(grainReferenceRuntime);

            this._grainCancelToken = (GrainCancellationToken)Activator.CreateInstance(typeof(GrainCancellationToken),
                                                                                      BindingFlags.NonPublic | BindingFlags.Instance,
                                                                                      null,
                                                                                      new object?[] { this.FlowUID, false, cancellRuntime },
                                                                                      null,
                                                                                      null)!;
        }

        /// <inheritdoc />
        void IExecutionContextInternal.AddCancelGrainReference(GrainReference grainRef)
        {
            if (this._grainCancelToken is null)
                ((IExecutionContextInternal)this).InitGrainCancelToken(grainRef);

            var grainReferenceRuntime = (IGrainReferenceRuntime)s_propGrainRuntime.GetValue(grainRef)!;

            var cancellationTokenRuntime = GetCancellationTokenRuntimeFromCache(grainReferenceRuntime);

            s_addGrainCancelReference.Invoke(this._grainCancelToken,
                                             new object?[]
                                             {
                                                    cancellationTokenRuntime.GetValue(grainReferenceRuntime),
                                                    grainRef
                                             });
        }

        /// <inheritdoc />  
        void IExecutionContextInternal.ForceGrainCancellationToken(GrainCancellationToken registredCancellationToken)
        {
            this._grainCancelToken = registredCancellationToken;
        }

        /// <summary>
        /// Gets the cancellation token runtime field info from cache or build data.
        /// </summary>
        private static FieldInfo GetCancellationTokenRuntimeFromCache(IGrainReferenceRuntime grainReferenceRuntime)
        {
            var traitRuntime = grainReferenceRuntime.GetType();

            s_grainReferenceRuntimeCacheLocker.EnterReadLock();
            try
            {
                if (s_grainReferenceRuntimeCache.TryGetValue(traitRuntime, out var fieldICachenfo))
                {
                    Debug.Assert(fieldICachenfo != null);
                    return fieldICachenfo;
                }
            }
            finally
            {
                s_grainReferenceRuntimeCacheLocker.ExitReadLock();
            }

            s_grainReferenceRuntimeCacheLocker.EnterWriteLock();
            try
            {
                if (s_grainReferenceRuntimeCache.TryGetValue(traitRuntime, out var fieldICachenfo))
                    return fieldICachenfo;

                var cancellationTokenRuntime = traitRuntime.GetField("cancellationTokenRuntime", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);

                Debug.Assert(cancellationTokenRuntime != null);
                s_grainReferenceRuntimeCache.Add(traitRuntime, cancellationTokenRuntime);

                return cancellationTokenRuntime;
            }
            finally
            {
                s_grainReferenceRuntimeCacheLocker.ExitWriteLock();
            }
        }

        #endregion

        #endregion
    }
}
