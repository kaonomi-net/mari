// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Cron
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Attributes;
    using MARI.Framework.Core.Abstractions.Triggers;
    using MARI.Framework.Node.Triggers;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    /// <summary>
    /// Virtual Grain Service instanciate and launch on all silo start used to start <see cref="ICronTriggerHandlerVGrain"/> by cron setups
    /// </summary>
    [MARISystemVGrain]
    internal sealed class CronVGrainService : TriggerBaseGrainService<ICronTriggerHandlerVGrain>, ICronVGrainService
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="CronVGrainService"/> class.
        /// </summary>
        public CronVGrainService(GrainId id,
                                 Silo silo,
                                 ILoggerFactory loggerFactory,
                                 IGrainOrleanFactory grainFactory,
                                 ITriggerDefinitionProvider triggerDefinitionProvider) 
            : base(id, silo, loggerFactory, grainFactory, triggerDefinitionProvider, TriggerTypeEnum.Cron)
        {
        }

        #endregion
    }
}
