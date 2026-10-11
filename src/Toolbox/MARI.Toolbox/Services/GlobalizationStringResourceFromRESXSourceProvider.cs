// Copyright (c) Kaonomi.
// This file is licenses to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Services
{
    using MARI.Toolbox.Abstractions.Services;

    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    /// <summary>
    /// Extract RESX information to expose them through <see cref="IGlobalizationStringResourceProvider"/>
    /// </summary>
    /// <seealso cref="IGlobalizationStringResourceProvider" />
    public sealed class GlobalizationStringResourceFromRESXSourceProvider<TResource> : IGlobalizationStringResourceSourceProvider
    {
        #region Fields

        private static readonly ResourceManager s_manager;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="GlobalizationStringResourceFromRESXSourceProvider{TResource}"/> class.
        /// </summary>
        static GlobalizationStringResourceFromRESXSourceProvider()
        {
            var resourceManagerProp = typeof(TResource).GetProperty("ResourceManager", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            if (resourceManagerProp is null)
                throw new InvalidDataException(typeof(TResource) + " must be a RESX file");

            s_manager = (ResourceManager)resourceManagerProp.GetValue(null)!;

        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool TryGetResource(string name, CultureInfo cultureInfo, out string value)
        {
            value = string.Empty;

            try
            {

                var result = s_manager.GetResourceSet(cultureInfo, true, true)?.GetString(name) ?? null;

                if (!string.IsNullOrEmpty(result))
                {
                    value = result;
                    return true;
                }

            }
            catch (MissingManifestResourceException _)
            {

            }

            return false;
        }

        #endregion
    }
}
