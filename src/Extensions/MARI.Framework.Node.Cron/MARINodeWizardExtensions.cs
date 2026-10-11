// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// Keep : MARI.Framework.Configurations; to easy configuration use
namespace MARI.Framework.Configurations
{
    using MARI.Framework.Node.Cron;
    using MARI.Toolbox.Extensions;

    using Microsoft.Extensions.DependencyInjection;

    public static class MARINodeWizardExtensions
    {
        /// <summary>
        /// Setup cron triggers into the mari node server.
        /// </summary>
        /// <param name="supportSecondCron">MARI support cron under minute time, but need more configuration to override the default orlean limit to 1 minute.</param>
        /// <remarks>
        ///     If you want second handling and custom the <see cref="ReminderOptions"/> plz add the <see cref="ReminderOptions.MinimumReminderPeriod"/> minimal value to 1 sec
        /// </remarks>
        public static IMARINodeWizard UseCronTriggers(this IMARINodeWizard mariNodeWizard, bool supportSecondCron = false)
        {
            mariNodeWizard.ConfigureServices(s =>
            {
                s.AddGrainService<CronVGrainService>()
                 .AddReminders();

                    // To support second timed cron we need to override the default option orleans reminders
                if (supportSecondCron)
                {
                    var reminderOptions = new ReminderOptions()
                    {
                        InitializationTimeout = TimeSpan.FromSeconds(2),
                        RefreshReminderListPeriod = TimeSpan.FromSeconds(2),
                        MinimumReminderPeriod = TimeSpan.FromSeconds(1),
                    };

                    s.AddSingleton(reminderOptions.ToOption())
                     .AddSingleton(reminderOptions.ToMonitorOption());
                }
            });
            return mariNodeWizard;
        }
    }
}
