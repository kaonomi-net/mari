// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions
{
    using MARI.Framework.Core.Abstractions.Exceptions;

    using MARI.Toolbox.Extensions;
    using MARI.Toolbox.Loggers;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using System.Linq;

    public static class DefinitionExtensions
    {
        #region Fields

        private static readonly IOptionsMonitor<LoggerFilterOptions> s_filterOption;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DefinitionExtensions"/> class.
        /// </summary>
        static DefinitionExtensions()
        {
            s_filterOption = (new LoggerFilterOptions() { MinLevel = LogLevel.Error }).ToMonitorOption();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Validates the with exception if error or critical
        /// </summary>
        public static void ValidateWithException(this IDefinition definition, ILogger? extraLogger = null)
        {
            // InMemoryLogger could not be reset, a new one is created per call to avoid mixing logs between validations
            var logger = new InMemoryLogger(s_filterOption);

            if (!definition.Validate(logger))
            {
                var logs = logger.GetLogsCopy();

                List<Exception>? errors = null;

                foreach (var log in logs)
                {
                    extraLogger?.OptiLog(log.LogLevel, log.Message);

                    if (log.LogLevel >= LogLevel.Error)
                    {
                        var ex = new DefinitionException(definition.GetType(), definition.Uid.ToString(), log.Message);
                        if (errors is null)
                            errors = new List<Exception>();
                        errors.Add(ex);
                    }
                }

                if (errors is not null && errors.Any())
                    throw new AggregateException(errors);
            }
        }

        #endregion
    }
}
