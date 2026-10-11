// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Extensions.Docker.Abstractions
{
    using global::Docker.DotNet;

    /// <summary>
    /// Abstraction above docker.dotnet client api
    /// </summary>
    public interface IDockerClientProxy : IDockerClient, IDisposable
    {
    }
}
