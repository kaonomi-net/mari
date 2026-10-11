// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.Steps
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Sequence;
    using MARI.Toolbox.Abstractions.Expressions;
    using MARI.Toolbox.Models;

    using System;

    /// <summary>
    /// Base class of <see cref="ISequencePipelineInternalStageStep"/>
    /// </summary>
    /// <seealso cref="ISequencePipelineInternalStageStep" />
    public abstract class StepBaseBuilder
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CallStepBuilder"/> class.
        /// </summary>
        protected StepBaseBuilder(Type? input, Type? output)
        {
            this.Input = input;
            this.Output = output;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the input.
        /// </summary>
        protected Type? Input { get; }

        /// <summary>
        /// Gets the output.
        /// </summary>
        protected Type? Output { get; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public abstract SequenceStageDefinition ToDefinition<TContext>(DefinitionMetaData? metaData,
                                                                       Guid uid,
                                                                       string displayName,
                                                                       bool preventReturn,
                                                                       AccessExpressionDefinition? configurationAccess,
                                                                       ConcretType? configurationFromContextDataType = null);

        #endregion
    }
}
