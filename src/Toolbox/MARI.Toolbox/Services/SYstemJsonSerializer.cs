// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Services
{
    using MARI.Toolbox.Abstractions.Services;

    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Implementation of <see cref="IJsonSerializer"/> used .net building serializer
    /// </summary>
    /// <seealso cref="IJsonSerializer" />
    public sealed class SystemJsonSerializer : IJsonSerializer
    {
        #region Fields

        private static readonly JsonSerializerOptions s_defaultDeserializationOptions;
        private readonly JsonSerializerOptions _deserializationOptions;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="SystemJsonSerializer"/> class.
        /// </summary>
        static SystemJsonSerializer()
        {
            s_defaultDeserializationOptions = new JsonSerializerOptions()
            {
                IncludeFields = true,
                PropertyNameCaseInsensitive = false,
                WriteIndented = Debugger.IsAttached,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
            };

            Instance = new SystemJsonSerializer();
            Default = (SystemJsonSerializer)Instance;
            WithoutJsonEscapte = new SystemJsonSerializer(new JsonSerializerOptions()
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                IncludeFields = true,
                PropertyNameCaseInsensitive = false,
                WriteIndented = Debugger.IsAttached,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
            });
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="SystemJsonSerializer"/> class from being created.
        /// </summary>
        public SystemJsonSerializer(JsonSerializerOptions? serializerOptions = null)
        {
            this._deserializationOptions = serializerOptions ?? s_defaultDeserializationOptions;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static IJsonSerializer Instance { get; }

        /// <summary>
        /// Gets the default.
        /// </summary>
        public static SystemJsonSerializer Default { get; }

        /// <summary>
        /// Gets the without json escapte.
        /// </summary>
        public static SystemJsonSerializer WithoutJsonEscapte { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public object? Deserialize(string json, Type returnType)
        {
            return JsonSerializer.Deserialize(json, returnType, this._deserializationOptions);
        }

        /// <inheritdoc />
        public object? Deserialize(Stream stream, Type returnType)
        {
            return JsonSerializer.Deserialize(stream, returnType, this._deserializationOptions);
        }

        /// <inheritdoc />
        public TResult? Deserialize<TResult>(Stream stream)
        {
            return JsonSerializer.Deserialize<TResult>(stream, this._deserializationOptions);
        }

        /// <inheritdoc />
        public TResult? Deserialize<TResult>(string json)
        {
            return JsonSerializer.Deserialize<TResult>(json, this._deserializationOptions);
        }

        /// <inheritdoc />
        public object? Deserialize(in ReadOnlySpan<byte> str, Type returnType)
        {
            return JsonSerializer.Deserialize(str, returnType, this._deserializationOptions);
        }

        /// <inheritdoc />
        public TResult? Deserialize<TResult>(in ReadOnlySpan<byte> str)
        {
            return JsonSerializer.Deserialize<TResult>(str, this._deserializationOptions);
        }

        /// <inheritdoc />
        public byte[] Serialize<TObject>(TObject obj)
        {
            var str = JsonSerializer.Serialize<TObject>(obj, this._deserializationOptions);
            return Encoding.UTF8.GetBytes(str);
        }

        #endregion
    }
}
