// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Services
{
    /// <summary>
    /// Serializer and deserializer service
    /// </summary>
    public interface ISerializer
    {
        #region Methods

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        byte[] Serialize<TObject>(TObject obj);

        /// <summary>
        /// Convert <paramref name="json"/> into c# object
        /// </summary>
        object? Deserialize(string str, Type returnType);

        /// <summary>
        /// Convert <paramref name="json"/> into c# object
        /// </summary>
        object? Deserialize(in ReadOnlySpan<byte> str, Type returnType);

        /// <summary>
        /// Convert <paramref name="stream"/> content into c# object
        /// </summary>
        object? Deserialize(Stream stream, Type returnType);

        /// <summary>
        /// Convert <paramref name="stream"/> content into c# <typeparamref name="TResult"/>
        /// </summary>
        TResult? Deserialize<TResult>(Stream stream);

        /// <summary>
        /// Convert <paramref name="json"/> into c# <typeparamref name="TResult"/>
        /// </summary>
        TResult? Deserialize<TResult>(string json);

        /// <summary>
        /// Convert <paramref name="json"/> into c# <typeparamref name="TResult"/>
        /// </summary>
        TResult? Deserialize<TResult>(in ReadOnlySpan<byte> str);

        #endregion
    }
}
