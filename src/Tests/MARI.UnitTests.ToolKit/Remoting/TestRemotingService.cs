// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.UnitTests.ToolKit.Remoting
{
    using MARI.Toolbox.Disposables;
    using MARI.Toolbox.Helpers;

    using Newtonsoft.Json;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Service used to managed all the required <see cref="IRemoteControllerService"/> declared
    /// </summary>
    /// <seealso cref="MARI.Toolbox.Disposables.SafeDisposable" />
    /// <seealso cref="MARI.UnitTests.ToolKit.Remoting.ITestRemotingService" />
    internal sealed class TestRemotingService : SafeDisposable, ITestRemotingService
    {
        #region Fields

        private readonly IReadOnlyCollection<IRemoteControllerService> _remoteControllers;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="TestRemotingService"/> class.
        /// </summary>
        static TestRemotingService()
        {
            SerializationSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRemotingService"/> class.
        /// </summary>
        public TestRemotingService(IEnumerable<IRemoteControllerService> remoteControllers)
        {
            this._remoteControllers = remoteControllers?.ToArray() ?? EnumerableHelper<IRemoteControllerService>.ReadOnlyArray;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the default serialization settings.
        /// </summary>
        public static JsonSerializerSettings SerializationSettings { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async ValueTask StartRemotingListnerAsync()
        {
            var controllerStartTask = this._remoteControllers.Select(r => r.InitializeRemoteAsync().AsTask()).ToArray();
            await Task.WhenAll(controllerStartTask);
        }

        /// <inheritdoc />
        protected override void DisposeBegin()
        {
            foreach (var dispo in this._remoteControllers.OfType<IDisposable>())
                dispo.Dispose();

            base.DisposeBegin();
        }

        #endregion
    }
}
