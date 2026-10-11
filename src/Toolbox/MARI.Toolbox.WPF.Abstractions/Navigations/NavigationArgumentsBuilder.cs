// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.Abstractions
{
    using MARI.Toolbox.WPF.Abstractions.Navigations;

    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    internal readonly struct NavigationArgumentsBuilder : INavigationArgumentsBuilder
    {
        #region Fields

        private readonly Dictionary<string, object?> _arguments;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationArgumentsBuilder"/> class.
        /// </summary>
        public NavigationArgumentsBuilder()
        {
            this._arguments = new Dictionary<string, object?>();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public INavigationArgumentsBuilder Add<TValue>(string key, TValue? value)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            this._arguments.Add(key, value);
#pragma warning restore CS8604 // Possible null reference argument.
            return this;
        }

        /// <summary>
        /// Converts to dictionary.
        /// </summary>
        public IDictionary<string, object?> ToDictionary()
        {
            return this._arguments;
        }

        #endregion
    }
}
