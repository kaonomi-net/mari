// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Models
{
    using MARI.Framework.Node.Blackboard.Abstractions.Models;

    [GenerateSerializer]
    public class DefaultBlackboardControllerState
    {
        /// <summary>
        /// Gets or sets the option.
        /// </summary>
        [Id(0)]
        public DefaultControllerOptions? Option { get; set; }   
    }
}
