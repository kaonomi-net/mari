// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Abstractions.VGrains
{
    using MARI.Framework.Node.Blackboard.Abstractions.Models;
    using MARI.Framework.Node.Blackboard.Abstractions.VGrains.Controllers;

    /// <summary>
    /// Dabic default controller mainly in charge to resolve data conflict
    /// </summary>
    /// <seealso cref="IBlackboardBaseControllerGrain" />
    public interface IDefaultBlackboardControllerGrain : IBlackboardStorageControllerGrain, IBlackboardBaseControllerGrain<DefaultControllerOptions>
    {
    }
}
