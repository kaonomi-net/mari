// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Serializations
{
    using MARI.Toolbox.Serializations.Converters;

    using Newtonsoft.Json;

    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    internal sealed class JsonSerializerSettingConfigBuilder : IJsonSerializerSettingConfigBuilder, IJsonSerializerConverterBuilder
    {
        #region Fields

        private readonly List<IMARIJsonObjectConverter> _converters;
        private IJsonSerializerSettingConfigBuilderInternal? _current;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerializerSettingConfigBuilder"/> class.
        /// </summary>
        public JsonSerializerSettingConfigBuilder()
        {
            this._converters = new List<IMARIJsonObjectConverter>();
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public IJsonSerializerConverterBuilder Converter
        {
            get { return this; }
        }

        /// <inheritdoc />
        public IJsonSerializerSettingConfigBuilder DoneConverter
        {
            get { return this; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IJsonSerializerConverterBuilder Add(IMARIJsonObjectConverter? jsonObjectConverter)
        {
            if (jsonObjectConverter is not null)
                this._converters.Add(jsonObjectConverter);
            return this;
        }

        /// <inheritdoc />
        public IJsonSerializerTypeResolverBuilder<TType> For<TType>()
        {
            this._current?.Build();
            this._current = null;

            var current = new JsonSerializerTypeResolverBuilder<TType>(this);
            this._current = current;

            return current;
        }

        /// <summary>
        /// Builds the specified settings.
        /// </summary>
        internal void Build(JsonSerializerSettings settings)
        {
            this._current?.Build();
            this._current = null;

            if (this._converters.Count > 0)
            {
                settings.Converters ??= new List<JsonConverter>();
                settings.Converters.Add(new NewtownJsonMetaConverter(this._converters));
            }
        }

        #endregion
    }
}
