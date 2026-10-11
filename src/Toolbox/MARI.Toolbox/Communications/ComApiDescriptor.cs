// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Communications
{
    using MARI.Toolbox.Abstractions.Attributes;

    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// 
    /// </summary>
    public sealed class ComApiDescriptor
    {
        #region Fields

        private readonly Dictionary<string, ApiHandler> _apiHandlers;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ComApiDescriptor"/> class.
        /// </summary>
        public ComApiDescriptor()
        {
            this._apiHandlers = new Dictionary<string, ApiHandler>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the handlers.
        /// </summary>
        internal IReadOnlyCollection<ApiHandler> Handlers
        {
            get { return this._apiHandlers.Values; }
        }

        #endregion

        #region Nested

        /// <summary>
        /// Handler descriptions
        /// </summary>
        internal sealed record class ApiHandler(string Route, Type MessageType, Delegate Callback, Type? ExpectResult);

        #endregion

        #region Methods

        /// <summary>
        /// Adds the API command hanlder.
        /// </summary>
        public ComApiDescriptor AddApiCommandHanlder<TMessage>(Func<TMessage, Guid, ValueTask> handler, string? route = null)
        {
            var messageType = typeof(TMessage);
            var rte = ExtractRoute(route, messageType);

            this._apiHandlers.Add(rte, new ApiHandler(rte, messageType, handler, null));
            return this;
        }

        /// <summary>
        /// Adds the API request hanlder.
        /// </summary>
        public ComApiDescriptor AddApiRequestHanlder<TMessage, TResult>(Func<TMessage, Guid, ValueTask<TResult>> handler, string? route = null)
        {
            var messageType = typeof(TMessage);
            var rte = ExtractRoute(route, messageType);

            this._apiHandlers.Add(rte, new ApiHandler(rte, messageType, handler, typeof(TResult)));
            return this;
        }

        /// <summary>
        /// Adds the API request hanlder.
        /// </summary>
        public ComApiDescriptor AddApiRequestDescription<TMessage, TResult>(string? route = null)
        {
            var messageType = typeof(TMessage);
            var rte = ExtractRoute(route, messageType);

            this._apiHandlers.Add(rte, new ApiHandler(rte, messageType, null, typeof(TResult)));
            return this;
        }

        #region Tools

        /// <summary>
        /// Extracts the Route.
        /// </summary>
        internal static string ExtractRoute(string? route, Type messageType)
        {
            return string.IsNullOrEmpty(route)
                         ? (string.IsNullOrEmpty(messageType.GetCustomAttribute<RefRouteAttribute>()?.Route)
                                  ? messageType.GetTypeInfoExtension().FullShortName!
                                  : messageType.GetCustomAttribute<RefRouteAttribute>()!.Route)
                         : route!;
        }

        #endregion
        #endregion
    }
}
