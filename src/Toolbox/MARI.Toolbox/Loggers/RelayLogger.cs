// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Loggers
{
    using MARI.Toolbox.Abstractions.Loggers;
    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.Logging;

    using System;

    /// <summary>
    /// Simple logger used to relay to another logger
    /// </summary>
    /// <seealso cref="ILogger" />
    public sealed class RelayLogger : SafeDisposable, ILogger
    {
        #region Fields

        private readonly InMemoryLogger? _localLogCopy;
        private readonly string _categoryName;
        private readonly ILogger _target;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayLogger"/> class.
        /// </summary>
        public RelayLogger(ILogger target, string categoryName, bool useLocalLogCopy = false)
        {
            this._target = target;
            this._categoryName = categoryName;

            if (useLocalLogCopy)
                this._localLogCopy = new InMemoryLogger(new LoggerFilterOptions() { MinLevel = LogLevel.Trace }.ToMonitorOption());
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return this._target.BeginScope(state);
        }

        /// <inheritdoc cref="InMemoryLogger.GetLogsCopy" />
        /// <returns>
        ///     Is not null only if the option have correctly be set at construction
        /// </returns>
        public IReadOnlyCollection<SimpleLog>? GetLogsCopy()
        {
            return this._localLogCopy?.GetLogsCopy();
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return this._target.IsEnabled(logLevel);
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var relayFormater = (TState s, Exception? e) => "[" + this._categoryName + "] " + formatter(s, e);
            this._target.Log<TState>(logLevel, eventId, state, exception, relayFormater);
            this._localLogCopy?.Log<TState>(logLevel, eventId, state, exception, relayFormater);
        }

        #endregion
    }
}
