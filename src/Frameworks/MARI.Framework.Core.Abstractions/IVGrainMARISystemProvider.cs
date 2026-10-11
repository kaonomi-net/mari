// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Services
{
    using MARI.Framework.Core.Abstractions;

    /// <summary>
    /// Provider that used the <see cref="IGrainOrleanFactory"/> to ensure no redirection
    /// </summary>
    /// <seealso cref="IVGrainProvider" />
    public interface IVGrainMARISystemProvider : IVGrainProvider
    {
    }
}
