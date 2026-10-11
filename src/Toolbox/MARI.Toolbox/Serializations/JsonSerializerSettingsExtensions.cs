// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Serializations
{
    using Newtonsoft.Json;

    /// <summary>
    /// Extend <see cref="JsonSerializerSettings"/>
    /// </summary>
    public static class JsonSerializerSettingsExtensions
    {
        /// <summary>
        /// Use fluent api to configure <see cref="JsonSerializerSettings"/>
        /// </summary>
        public static JsonSerializerSettings Configure(this JsonSerializerSettings settings, Action<IJsonSerializerSettingConfigBuilder> builder)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(builder);

            var configBuilder = new JsonSerializerSettingConfigBuilder();
            builder(configBuilder);
            configBuilder.Build(settings);

            return settings;
        }
    }
}
