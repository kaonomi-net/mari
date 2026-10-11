// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Builders.Signals
{
    /// <summary>
    /// Base builder shared by signal and signal
    /// </summary>
    /// <typeparam name="TWizard">The type of the wizard.</typeparam>
    public interface ISignalNetworkBasePartBuilder<TWizard>
        where TWizard : ISignalNetworkBasePartBuilder<TWizard>
    {
    }
}
