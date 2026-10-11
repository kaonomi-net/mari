// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Exceptions
{
    using MARI.Framework.Core.Abstractions;

    using System;

    /// <summary>
    /// Base exception during execution
    /// </summary>
    /// <seealso cref="MARIBaseException" />
    public abstract class MARIExecutionBaseException<TChild> : MARIBaseException<TChild>
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="MARIExecutionBaseException"/> class.
        /// </summary>
        public MARIExecutionBaseException(string message,
                                               Guid flowUid,
                                               Guid? parentExecutionId,
                                               Guid currentExecutionId,
                                               ulong errorCode,
                                               Exception? innerException = null)
            : base(message, errorCode, innerException)
        {
            this.Data.Add(nameof(IExecutionContext.FlowUID), flowUid);
            this.Data.Add(nameof(IExecutionContext.ParentExecutionId), parentExecutionId);
            this.Data.Add(nameof(IExecutionContext.CurrentExecutionId), currentExecutionId);
        }

        #endregion
    }
}
