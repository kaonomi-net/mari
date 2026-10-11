// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions
{
    using MARI.Framework.Core.Abstractions.Models.References;

    /// <summary>
    /// Setup the condition about an <typeparamref name="TVGrain"/> direct method call
    /// </summary>
    /// <typeparam name="TVGrain">The type of the vgrain.</typeparam>
    public interface IExecutionRefBuilder
    {
        /// <summary>
        /// Sets the configuration.
        /// </summary>
        IExecutionRefBuilder SetConfiguration<TConfig>(TConfig? config);

        /// <summary>
        /// Sets the input.
        /// </summary>
        IExecutionRefBuilder SetInput<TInput>(TInput? input);

        /// <summary>
        /// Setup the method that will be called using the vgrain with a specific return
        /// </summary>
        IExecutionRefLauncher Call(RefMethodQuery refMethod);
    }
}
