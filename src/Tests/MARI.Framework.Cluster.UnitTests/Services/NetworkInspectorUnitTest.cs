// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Cluster.UnitTests.Services
{
    using MARI.Framework.Cluster.Services;

    using NFluent;

    using System.Net;

    /// <summary>
    /// Test <see cref="NetworkInspector"/>
    /// </summary>
    public sealed class NetworkInspectorUnitTest
    {
        #region Methods

        /// <summary>
        /// Ensure ip string is not resolved
        /// </summary>
        [Theory]
        [InlineData("135.256.32.1", "135.256.32.1", false)]
        [InlineData("135.145.32.1:4256", "135.145.32.1:4256", true)]
        public void SolveHostAddresse_Resolve(string hostName, string? expected, bool ipEndpointParsed)
        {
            var inspector = new NetworkInspector();

            var results = inspector.SolveHostAddresse(hostName);

            if (expected != null)
            {
                Check.That(results).IsNotNull().And.CountIs(1).And.ContainsExactly(expected);

                if (ipEndpointParsed)
                {
                    var parseSuccess = IPEndPoint.TryParse(results.First(), out var parseIp);
                    Check.That(parseSuccess).IsTrue();
                    Check.That(parseIp).IsNotNull();
                    Check.That(parseIp!.Address + ":" + parseIp.Port).IsEqualTo(expected);
                }
            }
            else
            {
                Check.That(results).IsNotNull().And.IsEmpty();
            }
        }

        /// <summary>
        /// Ensure a host name is resolved to its IPv4 addresses with the port kept.
        /// The exact addresses returned for 'localhost' depend on the machine (IPv6, several 127.x entries...),
        /// so only loopback IPv4 endpoints with the requested port are expected.
        /// </summary>
        [Fact]
        public void SolveHostAddresse_Resolve_Localhost_Loopback()
        {
            var inspector = new NetworkInspector();

            var results = inspector.SolveHostAddresse("localhost:4256");

            Check.That(results).IsNotNull();

            var hasIpv4Loopback = Dns.GetHostAddresses("localhost")
                                     .Any(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && IPAddress.IsLoopback(ip));

            if (hasIpv4Loopback)
                Check.That(results).Not.IsEmpty();

            foreach (var result in results)
            {
                var parseSuccess = IPEndPoint.TryParse(result, out var parseIp);
                Check.That(parseSuccess).IsTrue();
                Check.That(parseIp).IsNotNull();
                Check.That(IPAddress.IsLoopback(parseIp!.Address)).IsTrue();
                Check.That(parseIp.Port).IsEqualTo(4256);
            }
        }

        #endregion
    }
}
