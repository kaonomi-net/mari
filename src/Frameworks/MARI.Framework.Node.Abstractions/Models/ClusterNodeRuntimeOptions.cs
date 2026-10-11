// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Models
{
    /// <summary>
    /// Define mari runtime cluster excution option
    /// </summary>
    /// <remarks>
    ///     Can be change on runtime
    /// </remarks>
    public sealed class ClusterNodeRuntimeOptions : INodeOptions
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterNodeRuntimeOptions"/> class.
        /// </summary>
        public ClusterNodeRuntimeOptions()
        {
            this.BlockSequenceExecutorStateStorageByDefault = false;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether allow or block sequence executor state to be saved or not by default.
        /// </summary>
        public bool BlockSequenceExecutorStateStorageByDefault { get; set; }

        #endregion
    }
}
