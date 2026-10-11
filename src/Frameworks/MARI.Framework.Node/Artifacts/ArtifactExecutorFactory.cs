// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Artifacts
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Artifacts;
    using MARI.Framework.Node.Abstractions.Artifacts;
    using MARI.Framework.Node.Abstractions.Exceptions;

    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <inheritdoc cref="IArtifactExecutorFactory" />
    /// <seealso cref="IArtifactExecutorFactory" />
    public sealed class ArtifactExecutorFactory : SafeDisposable, IArtifactExecutorFactory
    {
        #region Fields

        private readonly Dictionary<Guid, IArtifactExternalCodeExecutor> _cacheExec;
        private readonly ReaderWriterLockSlim _execCacheLocker;

        private readonly IReadOnlyCollection<IArtifactExecutorDedicatedFactory> _artifactExecutorDedicatedFactories;
        private readonly Dictionary<Guid, SemaphoreSlim> _lockByArtifact;
        private readonly SemaphoreSlim _lock;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactExecutorFactory"/> class.
        /// </summary>
        public ArtifactExecutorFactory(IEnumerable<IArtifactExecutorDedicatedFactory> artifactExecutorDedicatedFactories)
        {
            this._cacheExec = new Dictionary<Guid, IArtifactExternalCodeExecutor>();
            this._lockByArtifact = new Dictionary<Guid, SemaphoreSlim>();
            
            this._execCacheLocker = new ReaderWriterLockSlim();
            this._lock = new SemaphoreSlim(1);

            this._artifactExecutorDedicatedFactories = artifactExecutorDedicatedFactories?.ToArray() ?? EnumerableHelper<IArtifactExecutorDedicatedFactory>.ReadOnlyArray;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask<IArtifactExternalCodeExecutor> BuildAsync(ArtifactExecutableDefinition artifactCodePackageResource,
                                                                         IExecutionContext executionContext,
                                                                         ILogger logger,
                                                                         CancellationToken token)
        {
            SemaphoreSlim locker;
            await this._lock.WaitAsync(token);
            try
            {
                if (this._lockByArtifact.TryGetValue(artifactCodePackageResource.Uid, out var cachedLocker))
                {
                    locker = cachedLocker;
                }
                else
                {
                    locker = new SemaphoreSlim(1, 1);
                    this._lockByArtifact.Add(artifactCodePackageResource.Uid, locker);
                }
            }
            finally
            {
                this._lock.Release();
            }

            // In-process async lock by artifact (no thread blocked while waiting)
            await locker.WaitAsync(token);
            try
            {
                // Get a system (cross-process) lock to ensure the resources link to the artefact are not already used.
                // The named mutex is thread-affine, it is acquired and released on a dedicated thread to never block the caller scheduler.
                using (await AcquireSystemLockAsync(artifactCodePackageResource.Uid.ToString(), token))
                {
                    return await BuildInstallAsync(artifactCodePackageResource, executionContext, logger, token);
                }
            }
            finally
            {
                locker.Release();
            }
        }

        /// <summary>
        /// Acquires a named system mutex on a dedicated thread; the mutex is released (on the same thread) when the result is disposed.
        /// </summary>
        private static async Task<IDisposable> AcquireSystemLockAsync(string name, CancellationToken token)
        {
            var acquired = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var releaseSignal = new ManualResetEventSlim(false);

            var thread = new Thread(() =>
            {
                try
                {
                    using (var mutex = new Mutex(false, name))
                    {
                        try
                        {
                            var index = WaitHandle.WaitAny(new[] { mutex, token.WaitHandle });
                            if (index != 0)
                            {
                                acquired.TrySetCanceled(token);
                                return;
                            }
                        }
                        catch (AbandonedMutexException)
                        {
                            // Previous owner died without releasing, the mutex is now owned by this thread
                        }

                        try
                        {
                            acquired.TrySetResult();
                            releaseSignal.Wait();
                        }
                        finally
                        {
                            mutex.ReleaseMutex();
                        }
                    }
                }
                catch (Exception ex)
                {
                    acquired.TrySetException(ex);
                }
                finally
                {
                    releaseSignal.Dispose();
                }
            });

            thread.IsBackground = true;
            thread.Name = "ArtifactSystemLock:" + name;
            thread.Start();

            // If the acquisition fails or is cancelled the dedicated thread ends by itself
            await acquired.Task;

            return new DisposableAction(() => releaseSignal.Set());
        }

        /// <summary>
        /// Builds the install.
        /// </summary>
        private async ValueTask<IArtifactExternalCodeExecutor> BuildInstallAsync(ArtifactExecutableDefinition artifactCodePackageResource,
                                                                                 IExecutionContext executionContext,
                                                                                 ILogger logger,
                                                                                 CancellationToken token)
        {
            IArtifactExternalCodeExecutor? cachedExecutor = null;
            this._execCacheLocker.EnterReadLock();
            try
            {
                if (this._cacheExec.TryGetValue(artifactCodePackageResource.Uid, out var artifactExecutor))
                    cachedExecutor = artifactExecutor;
            }
            finally
            {
                this._execCacheLocker.ExitReadLock();
            }

            token.ThrowIfCancellationRequested();

            var dedicateFactory = this._artifactExecutorDedicatedFactories.FirstOrDefault(f => f.CanManaged(artifactCodePackageResource));

            if (dedicateFactory is null)
                throw new ArtifactPreparationFailedException("Missing dedicated artifact executor. " + artifactCodePackageResource.ToDebugDisplayName(), executionContext);

            if (cachedExecutor is not null)
            {
                if (await dedicateFactory.CheckExecutorValidityAsync(artifactCodePackageResource, cachedExecutor, executionContext, logger, token))
                    return cachedExecutor;

                await cachedExecutor.StopAsync(executionContext, logger, token);
            }

            var executor = await dedicateFactory.BuildNewExecutorAsync(artifactCodePackageResource, cachedExecutor, executionContext, logger, token);
            if (cachedExecutor is not null && object.ReferenceEquals(executor, cachedExecutor) == false)
                await cachedExecutor.DisposeAsync();

            this._execCacheLocker.EnterWriteLock();
            try
            {
                /*
                 * Due to dedicated locker by artifact Uid we are sure that previous (if exist) is not different from 'cachedExecutor' variable.
                 */
                this._cacheExec[artifactCodePackageResource.Uid] = executor;
                token.ThrowIfCancellationRequested();
            }
            finally
            {
                this._execCacheLocker.ExitWriteLock();
            }

            return executor;
        }

        /// <inheritdoc />
        protected override void DisposeEnd()
        {
            IArtifactExternalCodeExecutor[] executors;
            this._execCacheLocker.EnterWriteLock();
            try
            {
                executors = this._cacheExec.Values.ToArray();
                this._cacheExec.Clear();
            }
            finally
            {
                this._execCacheLocker.ExitWriteLock();
            }

            // Executors (daemon) keep external processes and TCP servers alive, they must be released.
            // Disposed on the thread pool to never capture the caller synchronization context; bounded to avoid blocking the shutdown forever.
            if (executors.Length > 0)
            {
                Task.Run(async () =>
                {
                    foreach (var executor in executors)
                    {
                        try
                        {
                            await executor.DisposeAsync();
                        }
                        catch
                        {
                            // Best effort on dispose
                        }
                    }
                }).Wait(TimeSpan.FromSeconds(30));
            }

            foreach (var locker in this._lockByArtifact.Values)
                locker.Dispose();
            this._lockByArtifact.Clear();

            this._execCacheLocker.Dispose();
            this._lock.Dispose();
            base.DisposeEnd();
        }

        #endregion
    }
}
