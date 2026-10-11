// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    /// <summary>
    /// Root of all builder used to configure a cluster part
    /// </summary>
    public interface IMARIBuilder
    {
    }

    /// <summary>
    /// Builder used to configure a cluster part
    /// </summary>
    public interface IMARIBuilder<TWizard, TWizardConfig> : IMARIBuilder
        where TWizard : IMARIWizard<TWizard, TWizardConfig>
        where TWizardConfig : IMARICoreConfigurationWizard<TWizardConfig>
    {
        /// <summary>
        /// Configurate the cluster node 
        /// </summary>
        IMARIWizardStart<TWizard, TWizardConfig> WizardConfig();

        /// <summary>
        /// Setups the cluster option.
        /// </summary>
        IMARIBuilder<TWizard, TWizardConfig> SetupClusterOption(Action<IClusterOptionBuilder> value);
    }
}
