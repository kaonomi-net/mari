// Copyright (c) Kaonomi.
// This file is licenses to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Helpers
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Models;

    using System;

    /// <summary>
    /// Helper used to easy manipulate the <see cref="ExecutionContext"/>
    /// </summary>
    public static class ExecutionContextHelper
    {
        /// <summary>
        /// Creates a new <see cref="IExecutionContext{TConfiguration}"/> from <paramref name="config"/>
        /// </summary>
        public static IExecutionContext<TConfig> CreateNew<TConfig>(TConfig config, Guid? flowUid = null, Guid? parentUid = null)
        {
            return new ExecutionContextWithConfiguration<TConfig>(flowUid ?? Guid.NewGuid(), Guid.NewGuid(), parentUid, config);
        }
    }
}
