// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI
{
    /// <summary>
    /// Define constants use by Blackboard extensions
    /// </summary>
    public static class MARIConstants
    {
        #region Fields

        public const string CSHARP_SYSTEM_NAMESPACE = "system";
        public const string SYSTEM_VGRAIN_NAMESPACE = "mari.system";

        #endregion

        #region ctor

        /// <summary>
        /// Initializes the <see cref="MARIConstants"/> class.
        /// </summary>
        static MARIConstants()
        {
            DefaultObserverTimeout = TimeSpan.FromMinutes(5);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The default dmocrite state storage configuration
        /// </summary>
        public const string DefaultDefinitionStorageName = "Definitions";

        /// <summary>
        /// The default dmocrite state storage configuration
        /// </summary>
        public const string DefaultMARIStateConfigurationKey = nameof(MARI);

        /// <summary>
        /// The default dmocrite dynamic definitions
        /// </summary>
        public const string DefaultMARIDynamicDefinitionsConfigurationKey = nameof(MARI) + "DynamicDefinitions";

        /// <summary>
        /// The default dmocrite dynamic definitions
        /// </summary>
        public const string DefaultMARIDynamicDefinitionsRepositoryConfigurationKey = DefaultMARIDynamicDefinitionsConfigurationKey + "Repository";

        /// <summary>
        /// The default dmocrite System state storage configuration
        /// </summary>
        public const string DefaultMARIAdminStateConfigurationKey = nameof(MARI) + "Admin";

        /// <summary>
        /// The default dmocrite repository storage configuration
        /// </summary>
        public const string DefaultMARIRepositoryConfigurationKey = nameof(MARI) + "Repository";

        /// <summary>
        /// Gets the default observer timeout.
        /// </summary>
        public static TimeSpan DefaultObserverTimeout { get; }

        #endregion
    }
}
