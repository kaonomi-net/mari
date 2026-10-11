// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Administrations
{
    using MARI.Framework.Node.Abstractions.Administrations;

    using Orleans.Services;

    /// <summary>
    /// Silo service used to align local service with administration configuration
    /// </summary>
    /// <seealso cref="IGrainService" />
    internal interface IAdministrationGrainService : IGrainService, IAdminEventReceiver
    {
    }
}
