// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.UnitTests.ToolKit.Remoting
{
    /// <summary>
    /// Define remote controller
    /// </summary>
    public interface ITestRemotingService : IDisposable
    {
        /// <summary>
        /// Starts remoting services
        /// </summary>
        ValueTask StartRemotingListnerAsync();
    }
}
