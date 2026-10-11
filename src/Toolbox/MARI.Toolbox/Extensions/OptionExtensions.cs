// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Extensions
{
    using MARI.Toolbox.Options;

    using Microsoft.Extensions.Options;

    /// <summary>
    /// Extensions linked to <see cref="Microsoft.Extensions.Options"/>
    /// </summary>
    public static class OptionExtensions
    {
        /// <summary>
        /// Convert <typeparamref name="TOptions"/> object to <see cref="IOptions{TOptions}"/>
        /// </summary>
        public static IOptions<TOptions> ToOption<TOptions>(this TOptions option)
            where TOptions : class
        {
            return Options.Create(option);
        }

        /// <summary>
        /// Convert <typeparamref name="TOptions"/> object to <see cref="IOptionsMonitor{TOptions}"/>
        /// </summary>
        public static IOptionsMonitor<TOptions> ToMonitorOption<TOptions>(this TOptions option)
            where TOptions : class
        {
            return new StaticMonitorOption<TOptions>(option);
        }
    }
}
