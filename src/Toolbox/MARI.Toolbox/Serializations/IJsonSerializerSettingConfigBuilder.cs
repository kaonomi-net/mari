// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Serializations
{
    /// <summary>
    /// Root builder of <see cref="JsonSerializerSettingsExtensions"/>
    /// </summary>
    public interface IJsonSerializerSettingConfigBuilder
    {
        /// <summary>
        /// Gets the converter.
        /// </summary>
        IJsonSerializerConverterBuilder Converter { get; }
    }

    /// <summary>
    /// Builder deficated to converters
    /// </summary>
    public interface IJsonSerializerConverterBuilder
    {
        /// <summary>
        /// Add a converter to solve concret type from a possible interface or abstract type
        /// </summary>
        IJsonSerializerTypeResolverBuilder<TType> For<TType>();

        /// <summary>
        /// Manually add <see cref="IMARIJsonObjectConverter"/>.
        /// </summary>
        IJsonSerializerConverterBuilder Add(IMARIJsonObjectConverter? jsonObjectConverter);

        /// <summary>
        /// End the current build session a turn back to root for a new type mapping
        /// </summary>
        IJsonSerializerSettingConfigBuilder DoneConverter { get; }
    }

    /// <summary>
    /// Internal object to build all the converters
    /// </summary>
    internal interface IJsonSerializerSettingConfigBuilderInternal
    {
        void Build();
    }
}
