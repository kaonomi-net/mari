// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.Abstractions.Navigations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// Navigation arguments
    /// </summary>
    public sealed class NavigationArguments
    {
        #region Fields

        private readonly IReadOnlyDictionary<string, object?> _arguments;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationArguments"/> class.
        /// </summary>
        private NavigationArguments(IDictionary<string, object?> arguments)
        {
            this._arguments = new ReadOnlyDictionary<string, object?>(arguments);
            this.Keys = this._arguments.Keys.ToArray();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the argument keys.
        /// </summary>
        public IReadOnlyCollection<string> Keys { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Try get arguments
        /// </summary>
        public bool TryGet<TArg>(string key, out TArg? arg)
        {
            arg = default;
            if (this._arguments.TryGetValue(key, out var objArg) && objArg is TArg castArg)
            {
                arg = castArg;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get required arguments
        /// </summary>
        public TArg GetRequired<TArg>(string key)
        {
            if (this._arguments.TryGetValue(key, out var objArg) && objArg is TArg castArg)
                return castArg;

            throw new InvalidDataException($"Missing required paramter '{key}' type {typeof(TArg)}");
        }

        /// <summary>
        /// Creates the specified build action.
        /// </summary>
        public static NavigationArguments Create(Action<INavigationArgumentsBuilder> buildAction)
        {
            var builder = new NavigationArgumentsBuilder();

            buildAction?.Invoke(builder);

            return new NavigationArguments(builder.ToDictionary());

        }

        #endregion
    }
}
