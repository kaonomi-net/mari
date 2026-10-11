// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Commands
{
    using System.Windows.Input;

    /// <summary>
    /// Extend the class <see cref="ICommand"/>
    /// </summary>
    /// <seealso cref="ICommand" />
    public interface IAsyncCommandExt : ICommandExt
    {
        /// <summary>
        /// Defines the async method to be called when the command is invoked.
        /// </summary>
        ValueTask ExecuteAsync();
    }

    /// <summary>
    /// Extend the class <see cref="ICommand"/> with a content <typeparamref name="TState"/>
    /// </summary>
    /// <seealso cref="ICommand" />
    public interface IAsyncCommandExt<TState> : ICommandExt<TState>
    {
        /// <summary>
        /// Defines the async method to be called when the command is invoked.
        /// </summary>
        ValueTask ExecuteAsync(TState state);
    }
}
