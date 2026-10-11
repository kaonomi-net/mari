// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]    
    public sealed class RefRouteAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RefRouteAttribute"/> class.
        /// </summary>
        public RefRouteAttribute(string route)
        {
            this.Route = route;                
        }

        /// <summary>
        /// Gets the route.
        /// </summary>
        public string Route { get; }
    }
}
