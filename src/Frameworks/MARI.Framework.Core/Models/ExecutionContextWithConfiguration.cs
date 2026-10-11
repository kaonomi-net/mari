// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Models
{
    using MARI.Framework.Core.Abstractions;

    using System;
    using System.ComponentModel;

    /// <summary>
    /// Define an execution context
    /// </summary>
    /// <seealso cref="IExecutionContext{TConfiguration}" />
    [Immutable]
    [Serializable]
    [ImmutableObject(true)]
    public sealed class ExecutionContextWithConfiguration<TConfiguration> : ExecutionContext, IExecutionContext<TConfiguration>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionContextWithConfiguration{TContextInfo}"/> class.
        /// </summary>
        public ExecutionContextWithConfiguration(Guid flowUID,
                                                 Guid currentExecutionId,
                                                 Guid? parentExecutionId,
                                                 TConfiguration? configuration = default)
            : base(flowUID, currentExecutionId, parentExecutionId)
        {
            this.Configuration = configuration ?? default;
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public TConfiguration? Configuration { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Simply create with just config
        /// </summary>
        public static IExecutionContext<TConfiguration> EmptyWithConfig(TConfiguration? configuration)
        {
            return new ExecutionContextWithConfiguration<TConfiguration>(Guid.Empty, Guid.Empty, null, configuration);
        }

        #endregion
    }
}
