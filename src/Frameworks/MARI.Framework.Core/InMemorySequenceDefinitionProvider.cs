// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Sequence;

    using System;

    /// <summary>
    /// <see cref="SequenceDefinition"/> provider register in memory
    /// </summary>
    /// <seealso cref="ISequenceDefinitionSourceProvider" />
    public sealed class InMemorySequenceDefinitionProvider : InMemoryBaseDefinitionProvider<SequenceDefinition>, ISequenceDefinitionSourceProvider
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemorySequenceDefinitionProvider"/> class.
        /// </summary>
        public InMemorySequenceDefinitionProvider(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        #endregion
    }
}
