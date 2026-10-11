// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.Sequences
{
    using Microsoft.Extensions.Logging;

    public interface ISequenceOptionBuilder
    {
        /// <summary>
        /// Set minimal log level
        /// </summary>
        ISequenceOptionBuilder MinimalLogLevel(LogLevel logLevel);

        /// <summary>
        /// Prevents the sequence executor state storage.
        /// </summary>
        ISequenceOptionBuilder PreventSequenceExecutorStateStorage();
    }
}
