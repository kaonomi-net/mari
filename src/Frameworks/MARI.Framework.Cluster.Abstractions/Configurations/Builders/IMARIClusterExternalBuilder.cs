// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    /// <summary>
    /// Builder used to setup cluster information
    /// </summary>
    /// <typeparam name="TOption">The type of the option.</typeparam>
    public interface IMARIClusterExternalBuilder<TOption> : IMARIExternalServiceBuilder<TOption, IMARIClusterExternalBuilder<TOption>>
        where TOption : class, new()
    {
    }
}
