// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.Sequences
{
    /// <summary>
    /// Used to build a trigger
    /// </summary>
    public interface ISequenceTriggerBuilder
    {
        /// <summary>
        /// Define the sequence need no input.
        /// </summary>
        public ISequencePipelineBuilder NoInput();

        /// <summary>
        /// Define the sequence required input <typeparamref name="TInput"/>
        /// </summary>
        public ISequencePipelineBuilder<TInput> RequiredInput<TInput>();
    }
}
