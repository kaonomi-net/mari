// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.UnitTests.ToolKit.Remoting
{
    /// <summary>
    /// Builder used to configure service pilot remotly
    /// </summary>
    public interface ITestRemotingServiceBuilder
    {
        /// <summary>
        /// Adds the singleton remoting virtually exposed to other silos.
        /// </summary>
        ITestRemotingServiceBuilder AddSingleton<TService>(TService service) where TService : class;

        /// <summary>
        /// Builds a controller that remote control all register services and provide info on it
        /// </summary>
        ITestRemotingService Build();
    }
}
