// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions
{
    /// <summary>
    /// Builder used to setup execution
    /// </summary>
    public interface IExecutionBuilder<TInput, TLauncher>
        where TLauncher : IExecutionLauncher
    {
        /// <summary>
        /// Sets the execution input.
        /// </summary>
        /// <remarks>
        ///     Last set remain
        /// </remarks>
        TLauncher SetInput(TInput? input);
    }

    /// <summary>
    /// Builder used to setup execution
    /// </summary>
    public interface IExecutionBuilder<TLauncher>
        where TLauncher : IExecutionLauncher
    {
        /// <summary>
        /// Sets the execution input.
        /// </summary>
        /// <remarks>
        ///     Last set remain
        /// </remarks>
        TLauncher SetInput(object? input);
    }
}
