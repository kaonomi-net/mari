// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Services
{
    using MARI.Framework.Core.Abstractions.Diagnostics;
    using MARI.Framework.Core.Abstractions.Enums;
    using MARI.Framework.Node.Abstractions.Models;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Orleans.Runtime;

    using System.Threading.Tasks;

    /// <summary>
    /// Connection service use to log incomming call to <see cref="IGrain"/>
    /// </summary>
    /// <seealso cref="GrainCallDiagnosticTracerService" />
    /// <seealso cref="IIncomingGrainCallFilter" />
    internal sealed class IncomingGrainCallTracer : GrainCallDiagnosticTracerService, IIncomingGrainCallFilter
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="IncomingGrainCallTracer"/> class.
        /// </summary>
        public IncomingGrainCallTracer(ILocalSiloDetails localSiloDetails,
                                       IOptions<ClusterNodeDiagnosticOptions> options,
                                       IDiagnosticLogger diagnosticLogger,
                                       ILoggerFactory loggerFactory)
            : base(localSiloDetails, options, diagnosticLogger, loggerFactory)
        {
        }

        #endregion

        /// <inheritdoc />
        public Task Invoke(IIncomingGrainCallContext context)
        {
            return base.Invoke(context, OrientationEnum.In);
        }
    }
}
