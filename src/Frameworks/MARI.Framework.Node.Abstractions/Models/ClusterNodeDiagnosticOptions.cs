// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Models
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Global cluser node options
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public sealed class ClusterNodeDiagnosticOptions : INodeOptions
    {
        #region Fields

        public const string ConfigurationName = "Diagnostic";

        [Obsolete("Misspelled, use " + nameof(ConfigurationName) + " instead.")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public const string ConfiguratioName = ConfigurationName;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ClusterNodeDiagnosticOptions"/> class.
        /// </summary>
        static ClusterNodeDiagnosticOptions()
        {
            Default = new ClusterNodeDiagnosticOptions(false, false, true, false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterNodeDiagnosticOptions"/> class.
        /// </summary>
        public ClusterNodeDiagnosticOptions()
            : this(Default.DisableVGrainExecutionTracing,
                   Default.TraceOrleanSystemVGrain,
                   Default.TraceMARISystemVGrain,
                   Default.DisableVGrainInOutTracing)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterNodeDiagnosticOptions"/> class.
        /// </summary>
        public ClusterNodeDiagnosticOptions(bool disableVGrainExecutionTracing,
                                            bool traceOrleanSystemVGrain,
                                            bool traceMARISystemVGrain,
                                            bool disableVGrainInOutTracing)
        {
            this.DisableVGrainExecutionTracing = disableVGrainExecutionTracing;
            this.TraceOrleanSystemVGrain = traceOrleanSystemVGrain;
            this.TraceMARISystemVGrain = traceMARISystemVGrain;
            this.DisableVGrainInOutTracing = disableVGrainInOutTracing;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default <see cref="ClusterNodeDiagnosticOptions"/>
        /// </summary>
        public static ClusterNodeDiagnosticOptions Default { get; }

        /// <summary>
        /// Gets or sets a value indicating whether vgrain execution tracing MUST be disable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if vgrain execution tracing; otherwise <c>false</c> enable tracing.
        /// </value>
        public bool DisableVGrainExecutionTracing { get; }

        /// <summary>
        /// Gets a value indicating whether we must trace orlean system vgrain.
        /// </summary>
        /// <value>
        ///   <c>true</c> if we must trace orlean system vgrain; otherwise <c>false</c>.
        /// </value>
        public bool TraceOrleanSystemVGrain { get; }

        /// <summary>
        /// Gets a value indicating whether we must trace mari system vgrain.
        /// </summary>
        /// <value>
        ///   <c>true</c> if we must trace mari system vgrain; otherwise, <c>false</c>.
        /// </value>
        public bool TraceMARISystemVGrain { get; }

        /// <summary>
        /// Gets or sets a value indicating whether vgrain input and output tracing MUST be disable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if vgrain intput and output tracing must be disabled; otherwise <c>false</c> enable tracing.
        /// </value>
        public bool DisableVGrainInOutTracing { get; }

        #endregion
    }
}
