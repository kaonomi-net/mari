// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Models
{
    using MARI.Framework.Core.Abstractions.Sequence;
    using MARI.Toolbox;
    using MARI.Toolbox.Abstractions.Supports;
    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Extensions;
    using MARI.Toolbox.Helpers;

    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// State used by <see cref="SequenceExecutor"/> to execute a state sequence
    /// </summary>
    [Serializable]
    internal sealed class SequenceExecutorExecThreadState : SafeDisposable,
                                                            ISupportDebugDisplayName,
                                                            IEquatable<SequenceExecutorExecThreadState>,
                                                            ISequenceExecutorExecThreadState
    {
        #region Fields

        private readonly ReaderWriterLockSlim _lock;

        private IReadOnlyCollection<SequenceExecutorExecThreadState> _innerThreads;
        private Guid _currentStageExecId;
        private Guid? _parentStageExecId;
        private Exception? _exception;
        private string? _errorMessage;
        private object? _output;
        private Guid? _cursor;
        private bool _started;
        private bool _done;
        private bool _running;

        #endregion

        #region Ctor

        /// <summary>
        /// 
        /// </summary>
        public SequenceExecutorExecThreadState(Guid flowUid,
                                               Guid flowDefinitionId,
                                               Guid currentStageExecId,
                                               Guid? parentStageExecId,
                                               SequenceDefinition? sequenceDefinition,
                                               object? threadInput)
            : this(flowUid,
                   flowDefinitionId,
                   currentStageExecId,
                   parentStageExecId,
                   sequenceDefinition?.Stages?.FirstOrDefault()?.Uid,
                   threadInput,
                   null,
                   null,
                   null,
                   false,
                   false)
        {
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="SequenceExecutorState"/> class from being created.
        /// </summary>
        [Newtonsoft.Json.JsonConstructor]
        [System.Text.Json.Serialization.JsonConstructor]
        internal SequenceExecutorExecThreadState(Guid flowUid,
                                                 Guid flowDefinitionId,
                                                 Guid currentStageExecId,
                                                 Guid? parentStageExecId,
                                                 Guid? cursor,
                                                 object? threadInput,
                                                 object? output,
                                                 IEnumerable<SequenceExecutorExecThreadState>? innerThreads,
                                                 Exception? exception,
                                                 bool started,
                                                 bool done,
                                                 string? errorMessage = null)
        {
            this._lock = new ReaderWriterLockSlim();

            this.FlowUid = flowUid;
            this.FlowDefinitionId = flowDefinitionId;
            this.ThreadInput = threadInput;

            this._cursor = cursor;
            this._output = output;
            this._exception = exception;
            this._errorMessage = errorMessage;
            this._started = started;
            this._done = done;

            this._currentStageExecId = currentStageExecId;
            this._parentStageExecId = parentStageExecId;

            this._innerThreads = innerThreads?.ToArray() ?? EnumerableHelper<SequenceExecutorExecThreadState>.ReadOnlyArray;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public Guid FlowUid { get; }

        /// <inheritdoc />
        public Guid FlowDefinitionId { get; }

        /// <inheritdoc />
        public Guid CurrentStageExecId
        {
            get
            {
                this._lock.EnterReadLock();
                try
                {
                    return this._currentStageExecId;
                }
                finally
                {
                    this._lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        public Guid? ParentStageExecId
        {
            get
            {
                this._lock.EnterReadLock();
                try
                {
                    return this._parentStageExecId;
                }
                finally
                {
                    this._lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        public Guid? Cursor
        {
            get
            {
                this._lock.EnterReadLock();
                try
                {
                    return this._cursor;
                }
                finally
                {
                    this._lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        public object? ThreadInput { get; }

        /// <inheritdoc />
        public object? Output
        {
            get
            {
                this._lock.EnterReadLock();
                try
                {
                    return this._output;
                }
                finally
                {
                    this._lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        [IgnoreDataMember]
        public bool JobDone
        {
            get
            {
                this._lock.EnterReadLock();
                try
                {
                    return this._done || ((this._exception != null || this._cursor == null) && (this.InnerThreads.Count == 0 || this.InnerThreads.All(i => i.JobDone)));
                }
                finally
                {
                    this._lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        [NotNull]
        public IReadOnlyCollection<SequenceExecutorExecThreadState> InnerThreads
        {
            get { return this._innerThreads; }
        }

        /// <inheritdoc />
        public string? ErrorMessage
        {
            get
            {
                this._lock.EnterReadLock();
                try
                {
                    return this._errorMessage;
                }
                finally
                {
                    this._lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        public Exception? Exception
        {
            get
            {
                this._lock.EnterReadLock();
                try
                {
                    return this._exception;
                }
                finally
                {
                    this._lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        public bool Started
        {
            get
            {
                this._lock.EnterReadLock();
                try
                {
                    return this._started;
                }
                finally
                {
                    this._lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc />
        public bool Running
        {
            get
            {
                this._lock.EnterReadLock();
                try
                {
                    return this._running;
                }
                finally
                {
                    this._lock.ExitReadLock();
                }
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public string ToDebugDisplayName()
        {
            var builder = new StringBuilder();
            builder.AppendLine();
            builder.Append($"[JobDone {this.JobDone}]");

            if (this.Exception != null)
            {
                builder.AppendLine();
                builder.Append($"[Exception:\n{this.Exception.GetFullString()}\n]");
            }

            if (this.InnerThreads != null && this.InnerThreads.Any())
            {
                foreach (var threadStr in this.InnerThreads.Select(t => t.ToDebugDisplayName()))
                {
                    builder.Append(' ', 4);
                    builder.Append(threadStr.Replace("\n", "\n" + string.Empty.PadRight(4)));
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Update execution state
        /// </summary>
        public void Update(Guid? cursor,
                           Guid nextStageExecId,
                           object? output,
                           IEnumerable<SequenceExecutorExecThreadState>? innerThreads)
        {
            this._lock.EnterWriteLock();
            try
            {
                this._cursor = cursor;
                this._output = output ?? NoneType.Instance;

                if (this._currentStageExecId != nextStageExecId)
                {
                    this._parentStageExecId = this._currentStageExecId;
                    this._currentStageExecId = nextStageExecId;
                }

                this._started = true;
                this._running = true;
                this._innerThreads = innerThreads?.ToArray() ?? EnumerableHelper<SequenceExecutorExecThreadState>.ReadOnlyArray;
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Sets state has done.
        /// </summary>
        public void SetJobIsDone(object? output)
        {
            this._lock.EnterWriteLock();
            try
            {
                this._cursor = null;
                this._output = output ?? NoneType.Instance;
                this._running = false;
                this._done = true;
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// End this thread with an error message
        /// </summary>
        public void SetError(string? error)
        {
            SetError(error, null);
        }

        /// <summary>
        /// End this thread with an <see cref="Exception"/>
        /// </summary>
        public void SetError(Exception? exception)
        {
            SetError(null, exception);
        }

        /// <summary>
        /// End this thread with an error message and a <see cref="Exception"/>
        /// </summary>
        public void SetError(string? message, Exception? exception)
        {
            this._lock.EnterWriteLock();
            try
            {
                Debug.Assert(string.IsNullOrEmpty(this._errorMessage) && this._exception == null, "ATTENTION : thread is already set as failed");

                this._errorMessage = message;
                this._exception = exception;
                this._cursor = null;
                this._running = false;
                this._done = true;
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return (this._cursor?.GetHashCode() ?? 0) ^
                   (this._output?.GetHashCode() ?? 0) ^
                   (this._exception?.GetHashCode() ?? 0) ^
                   (this._errorMessage?.GetHashCode() ?? 0) ^
                   this.FlowUid.GetHashCode() ^
                   this.FlowDefinitionId.GetHashCode() ^
                   (this.ThreadInput?.GetHashCode() ?? 0) ^
                   this._innerThreads.Aggregate(0, (acc, thread) => acc ^ thread.GetHashCode());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is SequenceExecutorExecThreadState state)
                return Equals(state);
            return false;
        }

        /// <inheritdoc />
        public bool Equals(SequenceExecutorExecThreadState? other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            // Snapshot values (each one under its own lock) to never hold two locks at the same time
            other.GetComparableSnapshot(out var otherCursor, out var otherErrorMessage, out var otherException, out var otherOutput, out var otherInnerThreads);
            GetComparableSnapshot(out var cursor, out var errorMessage, out var exception, out var output, out var innerThreads);

            return cursor == otherCursor &&
                   string.Equals(errorMessage, otherErrorMessage) &&
                   object.Equals(exception, otherException) &&
                   object.Equals(output, otherOutput) &&
                   this.FlowUid == other.FlowUid &&
                   this.FlowDefinitionId == other.FlowDefinitionId &&
                   object.Equals(this.ThreadInput, other.ThreadInput) &&
                   innerThreads.Count == otherInnerThreads.Count &&
                   innerThreads.All(i => otherInnerThreads.Any(ii => i.Equals(ii)));
        }

        /// <summary>
        /// Gets a consistent snapshot of the values used by <see cref="Equals(SequenceExecutorExecThreadState?)"/>
        /// </summary>
        private void GetComparableSnapshot(out Guid? cursor,
                                           out string? errorMessage,
                                           out Exception? exception,
                                           out object? output,
                                           out IReadOnlyCollection<SequenceExecutorExecThreadState> innerThreads)
        {
            this._lock.EnterReadLock();
            try
            {
                cursor = this._cursor;
                errorMessage = this._errorMessage;
                exception = this._exception;
                output = this._output;
                innerThreads = this._innerThreads;
            }
            finally
            {
                this._lock.ExitReadLock();
            }
        }

        /// <inheritdoc />
        protected override void DisposeManaged()
        {
            // Inner threads are not disposed here because the same instances can be shared by several thread states
            this._lock.Dispose();
            base.DisposeManaged();
        }

        #endregion
    }
}
