// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.UnitTests.ToolKit.Remoting
{
    using System.Threading.Tasks;

    /// <summary>
    /// Define remote services
    /// </summary>
    internal interface IRemoteControllerService
    {
        #region Properties

        /// <summary>
        /// Gets the uid.
        /// </summary>
        string Uid { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the remote server.
        /// </summary>
        ValueTask InitializeRemoteAsync(CancellationToken token = default);

        #endregion
    }
}
