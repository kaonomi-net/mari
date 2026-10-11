// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Extensions.Docker
{
    using MARI.Framework.Extensions.Docker.Abstractions;

    using global::Docker.DotNet;
    using global::Docker.DotNet.Models;

    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    internal sealed class DockerClientFactory : IDockerClientFactory, IDisposable
    {
        #region Fields

        private readonly static AuthConfig s_anonymousConfig;

        // Key : minimal version required (configuration name is not used yet to build the client)
        private readonly ConcurrentDictionary<string, Lazy<DockerClient>> _localClients;
        private int _disposed;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="DockerClientFactory"/> class.
        /// </summary>
        static DockerClientFactory()
        {
            s_anonymousConfig = new AuthConfig();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerClientFactory"/> class.
        /// </summary>
        public DockerClientFactory()
        {
            this._localClients = new ConcurrentDictionary<string, Lazy<DockerClient>>(StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IDockerClientProxy GetLocal(string? configurationName = null, string? minimalVersionRequired = null)
        {
            ObjectDisposedException.ThrowIf(this._disposed != 0, this);

            var key = minimalVersionRequired ?? string.Empty;

            var client = this._localClients.GetOrAdd(key,
                                                      static v => new Lazy<DockerClient>(() => new DockerClientConfiguration()
                                                                                                    .CreateClient(string.IsNullOrEmpty(v)
                                                                                                                        ? null
                                                                                                                        : System.Version.Parse(v)),
                                                                                         LazyThreadSafetyMode.ExecutionAndPublication)).Value;

            return new DockerClientProxy(client);
        }

        /// <inheritdoc />
        public AuthConfig GetRepositoryAuthentication(string? repository, string? configurationName)
        {
            // TODO : authentication by repo
            return s_anonymousConfig;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.Exchange(ref this._disposed, 1) != 0)
                return;

            foreach (var client in this._localClients.Values)
            {
                if (client.IsValueCreated)
                    client.Value.Dispose();
            }

            this._localClients.Clear();
        }

        #endregion
    }
}
