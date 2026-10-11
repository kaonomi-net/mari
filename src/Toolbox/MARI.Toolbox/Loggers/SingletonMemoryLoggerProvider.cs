// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Loggers
{
    using MARI.Toolbox.Abstractions.Loggers;
    using MARI.Toolbox.Disposables;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Provider used to relay all log to a single <see cref="InMemoryLogger{TCategoryName}"/>
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Logging.ILoggerProvider" />
    internal sealed class SingletonMemoryLoggerProvider : SafeDisposable, ILoggerProvider
    {
        #region Fields

        private readonly IInMemoryLogger _memoryLogger;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonMemoryLoggerProvider"/> class.
        /// </summary>
        public SingletonMemoryLoggerProvider(IInMemoryLogger inMemoryLogger)
        {
            this._memoryLogger = inMemoryLogger;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName)
        {
            return new RelayLogger(this._memoryLogger, categoryName);
        }

        #endregion
    }
}
