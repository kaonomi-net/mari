// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Serializations.Converters
{
    using MARI.Toolbox;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using System;

    /// <summary>
    /// Meta converter as proxy to <see cref="IMARIJsonObjectConverter"/>
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.JsonConverter" />
    internal sealed class NewtownJsonMetaConverter : JsonConverter
    {
        #region Fields

        private readonly IReadOnlyCollection<IMARIJsonObjectConverter> _converters;

        #endregion

        #region Ctor

        /// <summary>
        /// Prevents a default instance of the <see cref="NewtownJsonMetaConverter"/> class from being created.
        /// </summary>
        internal NewtownJsonMetaConverter(IEnumerable<IMARIJsonObjectConverter> converters)
        {
            this._converters = converters?.ToArray() ?? EnumerableHelper<IMARIJsonObjectConverter>.ReadOnly;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return this._converters.Any(c => c.CanConvert(objectType));
        }

        /// <inheritdoc />
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var initialObjectType = objectType;

            var converter = this._converters.FirstOrDefault(c => c.CanConvert(objectType) && c.CanRead);

            if (converter is not null)
            {
                var obj = JObject.Load(reader);

                var success = converter.ReadJson((prop) =>
                {
                    var exist = obj.TryGetValue(prop, out var value);

                    object? propValue = null;
                    if (value is JProperty jprop)
                        propValue = jprop.Value.Value<object>();
                    else if (value is JValue jvalue)
                        propValue = jvalue.Value;
                    else
                        propValue = value?.Value<object>();

                    return Tuple.Create(exist, propValue);
                },
                ref objectType,
                ref existingValue);

                if (success)
                    return existingValue;

                // Type not solved, deserializing again with the same type will route back into this converter (infinite recursion)
                if (initialObjectType == objectType)
                    throw new InvalidOperationException("Could not solve concrete type to deserialize item type " + objectType + " Path " + reader.Path);

                var objReader = obj.CreateReader();
                return serializer.Deserialize(objReader, objectType);
            }

            if (initialObjectType == objectType)
                throw new InvalidOperationException("Could not deserialize item type " + objectType + " Path " + reader.Path);

            return serializer.Deserialize(reader, objectType);
        }

        /// <inheritdoc />
        public override bool CanWrite
        {
            get { return false; }
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotSupportedException("NewtownJsonMetaConverter should only be used while deserializing.");
        }

        #endregion
    }
}
