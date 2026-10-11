// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Toolbox.Abstractions.Disposables;

    /// <summary>
    /// Internal thread executor
    /// </summary>
    internal interface ISequenceExecutorExecThread
    {
        #region Properties

        /// <summary>
        /// Gets the current execution context.
        /// </summary>
        IExecutionContext ExecutionContext { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the security thread handler.
        /// </summary>
        ISecureContextToken<ISequenceExecutorThreadHandler> GetSecurityThreadHandler();

        #endregion
    }
}
