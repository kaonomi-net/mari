// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Services
{
    using MARI.Toolbox.Abstractions.Services;

    using Newtonsoft.Json;

    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Implementation of <see cref="IJsonSerializer"/> used .net building serializer
    /// </summary>
    /// <seealso cref="IJsonSerializer" />
    public sealed class NewtownJsonSerializer : IJsonSerializer
    {
        #region Fields

        private static readonly JsonSerializerSettings s_defaultDeserializationOptions;
        private readonly JsonSerializerSettings _deserializationOptions;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="NewtownJsonSerializer"/> class.
        /// </summary>
        static NewtownJsonSerializer()
        {
            s_defaultDeserializationOptions = new JsonSerializerSettings()
            {
                //TO-DO from Opus 5.5 Default TypeNameHandling.Auto enables RCE gadgets : The default Instance deserializes any $type found in the JSON without an ISerializationBinder. Untrusted input (for example ComApiHandler messages coming over TCP) can therefore create arbitrary gadget types. Default to TypeNameHandling.None, or add a strict allow-list SerializationBinder.
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Debugger.IsAttached ? Formatting.Indented : Formatting.None,
            };

            Instance = new NewtownJsonSerializer();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="NewtownJsonSerializer"/> class from being created.
        /// </summary>
        public NewtownJsonSerializer(JsonSerializerSettings? serializerOptions = null)
        {
            this._deserializationOptions = serializerOptions ?? s_defaultDeserializationOptions;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static IJsonSerializer Instance { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public object? Deserialize(string json, Type returnType)
        {
            return JsonConvert.DeserializeObject(json, returnType, this._deserializationOptions);
        }

        /// <inheritdoc />
        public object? Deserialize(Stream stream, Type returnType)
        {
            using (var reader = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject(reader.ReadToEnd(), returnType, this._deserializationOptions);
            }
        }

        /// <inheritdoc />
        public TResult? Deserialize<TResult>(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject<TResult>(reader.ReadToEnd(), this._deserializationOptions);
            }
        }

        /// <inheritdoc />
        public TResult? Deserialize<TResult>(string json)
        {
            return JsonConvert.DeserializeObject<TResult>(json, this._deserializationOptions);
        }

        /// <inheritdoc />
        public object? Deserialize(in ReadOnlySpan<byte> str, Type returnType)
        {
            return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(str), returnType, this._deserializationOptions);
        }

        /// <inheritdoc />
        public TResult? Deserialize<TResult>(in ReadOnlySpan<byte> str)
        {
            return JsonConvert.DeserializeObject<TResult>(Encoding.UTF8.GetString(str), this._deserializationOptions);
        }

        /// <inheritdoc />
        public byte[] Serialize<TObject>(TObject obj)
        {
            var str = JsonConvert.SerializeObject(obj, this._deserializationOptions);
            return Encoding.UTF8.GetBytes(str);
        }

        #endregion
    }
}
