// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// Keep : System.Threading.Tasks
namespace System.Threading.Tasks
{
    using MARI.Toolbox.Abstractions.Extensions.Types;
    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Extend task
    /// </summary>
    public static class TaskExtensions
    {
        #region Fields

        private static readonly Dictionary<Type, MethodInfo> s_taskFromResultCache;
        private static readonly ReaderWriterLockSlim s_taskFromLocker;
        private static readonly MethodInfo s_taskFromResultMthd;

        #endregion

        #region Ctor

        /// <summary>
        /// Initialize the class <see cref="TaskExtensions"/>
        /// </summary>
        static TaskExtensions()
        {
            s_taskFromResultMthd = typeof(Task).GetMethod(nameof(Task.FromResult), BindingFlags.Static | BindingFlags.Public) ?? throw new ArgumentNullException("Missing " + nameof(Task.FromResult));

            s_taskFromResultCache = new Dictionary<Type, MethodInfo>();
            s_taskFromLocker = new ReaderWriterLockSlim();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets task result
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TResult? GetResult<TResult>(this Task task)
        {
            ArgumentNullException.ThrowIfNull(task);

            if (task is Task<TResult> taskResult)
                return taskResult.GetAwaiter().GetResult();

            return (TResult?)GetResult(task);
        }

        /// <summary>
        /// Gets task result
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? GetResult(this Task inst)
        {
            ArgumentNullException.ThrowIfNull(inst);

            var typeInfo = inst.GetType().GetTypeInfoExtension()!;

            return typeInfo.GetSpecifcTypeExtend<ITaskTypeInfoEnhancer>().GetResult(inst);
        }

        /// <summary>
        /// Gets <see cref="Task"/> from any <see cref="ValueTask"/> or <see cref="ValueTask{TResult}"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task GetTaskFromAnyValueTask(this ITypeInfoExtension? valueTaskInfo, object? inst)
        {
            if (valueTaskInfo is null)
                return Task.CompletedTask;

            if (!valueTaskInfo.IsValueTask)
                throw new InvalidCastException("Must only target value task");

            return valueTaskInfo.GetSpecifcTypeExtend<IValueTaskTypeInfoEnhancer>().AsTask(inst);
        }

        /// <summary>
        /// Gets the task result from Task.FromResult .
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task GetTaskFrom(this object? data, Type? forceType = null)
        {
            var type = forceType ?? data?.GetType() ?? throw new InvalidCastException("TaskExtensions.GetTaskFrom : No type could be detected plz enter one manually");
            var taskFromBuiler = s_taskFromResultMthd;

            s_taskFromLocker.EnterReadLock();
            try
            {
                if (s_taskFromResultCache.TryGetValue(type, out var result))
                    taskFromBuiler = result;
            }
            finally
            {
                s_taskFromLocker.ExitReadLock();
            }

            if (taskFromBuiler == null || taskFromBuiler == s_taskFromResultMthd)
            {
                s_taskFromLocker.EnterWriteLock();
                try
                {
                    taskFromBuiler = s_taskFromResultMthd.MakeGenericMethod(type);

                    if (!s_taskFromResultCache.ContainsKey(type))
                        s_taskFromResultCache.Add(type, taskFromBuiler);
                }
                finally
                {
                    s_taskFromLocker.ExitWriteLock();
                }
            }

            var task = (Task?)taskFromBuiler.Invoke(null, new[] { data });
            Debug.Assert(task != null);
            return task;
        }

        /// <summary>
        /// Safes wait all tasks to be completed
        /// </summary>
        /// <exception cref="AggregateException"></exception>
        public static async Task SafeWhenAllAsync(this IReadOnlyCollection<ValueTask> tasks, CancellationToken token = default)
        {
            await SafeWhenAllAsync(tasks.Select(t => t.AsTask()).ToReadOnly(), token);
        }

        /// <summary>
        /// Safes wait all tasks to be completed
        /// </summary>
        /// <exception cref="AggregateException"></exception>
        public static async Task<IReadOnlyCollection<TData>> SafeWhenAllWithResultsAsync<TData>(this IReadOnlyCollection<ValueTask<TData>> tasks, CancellationToken token = default, ILogger? logger = null)
        {
            logger ??= NullLogger.Instance;

            // A ValueTask must be consumed only once : convert each one to a Task once and only use the Tasks after
            var asTasks = tasks.Select(t => t.AsTask()).ToArray();

            await SafeWhenAllAsync((IReadOnlyCollection<Task>)asTasks, token);

            return await AggregateResults<TData>(asTasks, logger);
        }

        /// <summary>
        /// Safes wait all tasks to be completed
        /// </summary>
        /// <exception cref="AggregateException"></exception>
        public static async Task SafeWhenAllAsync<TData>(this IReadOnlyCollection<ValueTask<TData>> tasks, CancellationToken token = default)
        {
            await SafeWhenAllAsync(tasks.Select(t => t.AsTask()).ToReadOnly(), token);
        }

        /// <summary>
        /// Safes wait all tasks to be completed
        /// </summary>
        /// <exception cref="AggregateException"></exception>
        public static async Task SafeWhenAllAsync(this IReadOnlyCollection<Task> tasks, CancellationToken token = default)
        {
            if (tasks is null || tasks.Count == 0)
                return;

            var remains = new HashSet<Task>(tasks);

            while (remains.Any())
            {
                try
                {
                    await Task.WhenAny(remains);
                }
                catch
                {
                }

                if (token.IsCancellationRequested)
                    break;

                remains.RemoveWhere(x => x.IsCompleted);
            }

            var exceptions = tasks.Where(t => t.IsFaulted && !t.IsCanceled && t.Exception != null)
                                  .Select(e => e.Exception!)
                                  .Distinct()
                                  .ToArray() ?? EnumerableHelper<Exception>.ReadOnlyArray;

            if (exceptions.Any())
                throw new AggregateException(exceptions);

            token.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Gets the results from tasks.
        /// </summary>
        public static async ValueTask<IReadOnlyCollection<TTaskResult>> SafeWhenAllWithResultsAsync<TTaskResult>(this IReadOnlyCollection<Task<IReadOnlyCollection<TTaskResult>>> tasks, ILogger? logger = null)
        {
            logger ??= NullLogger.Instance;

            var tasksCollection = tasks.Where(t => t.IsCompleted == false);

            while (tasksCollection.Any())
            {
                try
                {
                    await Task.WhenAll(tasksCollection);
                }
                catch
                {
                }
                finally
                {
                }
            }

            return await AggregateResults(tasks, logger);
        }

        /// <summary>
        /// Gets the results from tasks.
        /// </summary>
        public static async ValueTask<IReadOnlyCollection<TTaskResult>> SafeWhenAllWithResultsAsync<TTaskResult>(this IReadOnlyCollection<ValueTask<IReadOnlyCollection<TTaskResult>>> tasks, ILogger? logger = null)
        {
            logger ??= NullLogger.Instance;

            // A ValueTask must be consumed only once : convert each one to a Task once and only use the Tasks after
            var asTasks = tasks.Select(t => t.AsTask()).ToArray();

            try
            {
                await Task.WhenAll(asTasks);
            }
            catch
            {
            }

            var results = new HashSet<TTaskResult>(tasks.Count * 2);
            foreach (var task in asTasks)
            {
                if (task.IsCompletedSuccessfully)
                {
                    foreach (var taskResult in task.Result)
                        results.Add(taskResult);
                }
                else if (task.IsCanceled)
                {
                    continue;
                }
                else if (task.Exception != null)
                {
                    logger.OptiLog(LogLevel.Error,
                                   "Aggregate - exception : {exception}]",
                                   task.Exception);
                }
            }

            return results;
        }

        /// <summary>
        /// Aggregates the results.
        /// </summary>
        public static ValueTask<IReadOnlyCollection<TTaskResult>> AggregateResults<TTaskResult>(this IReadOnlyCollection<ValueTask<TTaskResult>> tasks, ILogger? logger = null)
        {
            return AggregateResultImpl(tasks, logger);
        }

        /// <summary>
        /// Aggregates the results.
        /// </summary>
        public static async ValueTask<IReadOnlyCollection<TTaskResult>> AggregateResults<TTaskResult>(this IReadOnlyCollection<ValueTask<IReadOnlyCollection<TTaskResult>>> tasks, ILogger? logger = null)
        {
            var results = await AggregateResultImpl<IReadOnlyCollection<TTaskResult>>(tasks, logger);
            return results.SelectMany(s => s)
                          .Distinct()
                          .ToArray();
        }

        /// <summary>
        /// Aggregates the results.
        /// </summary>
        public static ValueTask<IReadOnlyCollection<TTaskResult>> AggregateResults<TTaskResult>(this IReadOnlyCollection<Task<TTaskResult>> tasks, ILogger? logger = null)
        {
            return AggregateResultImpl(tasks, logger);
        }

        /// <summary>
        /// Aggregates the results.
        /// </summary>
        public static async ValueTask<IReadOnlyCollection<TTaskResult>> AggregateResults<TTaskResult>(this IReadOnlyCollection<Task<IReadOnlyCollection<TTaskResult>>> tasks, ILogger? logger = null)
        {
            var results = await AggregateResultImpl<IReadOnlyCollection<TTaskResult>>(tasks, logger);
            return results.SelectMany(s => s)
                          .Distinct()
                          .ToArray();
        }

        #region Tools

        private static ValueTask<IReadOnlyCollection<TTaskResult>> AggregateResultImpl<TTaskResult>(this IReadOnlyCollection<ValueTask<TTaskResult>> tasks, ILogger? logger = null)
        {
            logger ??= NullLogger.Instance;

            var results = new HashSet<TTaskResult>(tasks.Count + 1);
            foreach (var task in tasks)
            {
                if (task.IsCompletedSuccessfully)
                {
                    results.Add(task.Result);
                }
                else if (task.IsCanceled)
                {
                    continue;
                }
                else if (task.IsFaulted)
                {
                    // Consume the ValueTask only once
                    var exception = task.AsTask().Exception;
                    if (exception != null)
                    {
                        logger.OptiLog(LogLevel.Error,
                                       "Aggregate - exception : {exception}]",
                                       exception);
                    }
                }
            }

            return ValueTask.FromResult(results.ToReadOnly());
        }

        private static ValueTask<IReadOnlyCollection<TTaskResult>> AggregateResultImpl<TTaskResult>(this IReadOnlyCollection<Task<TTaskResult>> tasks, ILogger? logger = null)
        {
            logger ??= NullLogger.Instance;

            var results = new HashSet<TTaskResult>(tasks.Count + 1);
            foreach (var task in tasks)
            {
                if (task.IsCompletedSuccessfully)
                {
                    results.Add(task.Result);
                }
                else if (task.IsCanceled)
                {
                    continue;
                }
                else if (task.Exception != null)
                {
                    logger.OptiLog(LogLevel.Error,
                                   "Aggregate - exception : {exception}]",
                                   task.Exception);
                }
            }

            return ValueTask.FromResult(results.ToReadOnly());
        }

        #endregion

        #endregion
    }
}
