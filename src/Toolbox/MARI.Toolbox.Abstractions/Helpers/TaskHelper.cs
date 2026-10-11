// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using static System.Runtime.InteropServices.JavaScript.JSType;

    /// <summary>
    /// Helper around task handling
    /// </summary>
    public static class TaskHelper
    {
        #region Nested

        /// <summary>
        /// Host of Task with always the same result
        /// </summary>
        private static class FixedResultTask<T>
            where T : notnull, IEquatable<T>
        {
            #region Fields

            private static readonly ReaderWriterLockSlim s_accessLocker;
            private static readonly Dictionary<T, Task<T>> s_cachedTasks;

            #endregion

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="FixedResultTask{T}"/> class.
            /// </summary>
            static FixedResultTask()
            {
                s_cachedTasks = new Dictionary<T, Task<T>>();
                s_accessLocker = new ReaderWriterLockSlim();
            }

            #endregion

            #region Methods

            /// <summary>
            /// Clears all.
            /// </summary>
            public static void ClearAll()
            {
                s_accessLocker.EnterWriteLock();
                try
                {
                    s_cachedTasks.Clear();
                }
                finally
                {
                    s_accessLocker.ExitWriteLock();
                }
            }

            /// <summary>
            /// Clears from result cache.
            /// </summary>
            public static void ClearFromResultCache(T Data)
            {
                s_accessLocker.EnterWriteLock();
                try
                {
                    s_cachedTasks.Remove(Data);
                }
                finally
                {
                    s_accessLocker.ExitWriteLock();
                }
            }

            /// <summary>
            /// Gets the or create.
            /// </summary>
            public static Task<T> GetOrCreate(T data)
            {
                s_accessLocker.EnterReadLock();
                try
                {
                    if (s_cachedTasks.TryGetValue(data, out var task))
                        return task;
                }
                finally
                {
                    s_accessLocker.ExitReadLock();
                }

                s_accessLocker.EnterWriteLock();
                try
                {
                    if (s_cachedTasks.TryGetValue(data, out var task))
                        return task;

                    var newTask = Task.FromResult<T>(data);
                    s_cachedTasks.Add(data, newTask);

                    return newTask;
                }
                finally
                {
                    s_accessLocker.ExitWriteLock();
                }
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets <see cref="Task{TResult}"/> obtains from <see cref="Task.FromResult{TResult}(TResult)"/>, the return task is cached to prevent to many task creations
        /// </summary>
        public static Task<T> GetFromResultCache<T>(T data)
            where T : notnull, IEquatable<T>
        {
            return TaskHelper.FixedResultTask<T>.GetOrCreate(data);
        }

        /// <summary>
        /// Gets <see cref="Task{TResult}"/> obtains from <see cref="Task.FromResult{TResult}(TResult)"/>, the return task is cached to prevent to many task creations
        /// </summary>
        public static void ClearFromResultCache<T>(T data)
            where T : notnull, IEquatable<T>
        {
            TaskHelper.FixedResultTask<T>.ClearFromResultCache(data);
        }

        /// <summary>
        /// Gets <see cref="Task{TResult}"/> obtains from <see cref="Task.FromResult{TResult}(TResult)"/>, the return task is cached to prevent to many task creations
        /// </summary>
        public static void ClearAllFromResultCache<T>()
            where T : notnull, IEquatable<T>
        {
            TaskHelper.FixedResultTask<T>.ClearAll();
        }

        #endregion
    }
}
