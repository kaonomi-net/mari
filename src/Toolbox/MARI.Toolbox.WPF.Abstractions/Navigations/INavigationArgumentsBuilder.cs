// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.Abstractions.Navigations
{
    /// <summary>
    /// 
    /// </summary>
    public interface INavigationArgumentsBuilder
    {
        /// <summary>
        /// Adds specified value in the navigation arguments
        /// </summary>
        INavigationArgumentsBuilder Add<TValue>(string key, TValue? value);
    }
}
