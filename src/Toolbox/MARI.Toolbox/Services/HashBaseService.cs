// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Services
{
    using MARI.Toolbox.Abstractions.Services;

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Service in charge to provide SHA256 hashage 
    /// </summary>
    /// <seealso cref="IHashService" />
    public abstract class HashBaseService : IHashService
    {
        #region Fields

        private readonly Func<HashAlgorithm> _hashAlgorithmFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="HashBaseService"/> class.
        /// </summary>
        protected HashBaseService(Func<HashAlgorithm> hashAlgorithmFactory)
        {
            this._hashAlgorithmFactory = hashAlgorithmFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public ValueTask<string> GetHash(string data,
                                         Encoding? encoding = null,
                                         CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(data);

            var encodingAlgo = encoding ?? Encoding.UTF8;

            return GetHash(encodingAlgo.GetBytes(data), token);
        }

        /// <inheritdoc />
        public async ValueTask<string> GetHash(byte[] data,
                                               CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(data);

            // Await inside the using block : the stream must stay alive until the hash is computed
            using (var stream = new MemoryStream(data))
            {
                return await GetHash(stream, token);
            }
        }

        /// <inheritdoc />
        public async ValueTask<string> GetHash(Stream stream,
                                               CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (stream.CanSeek == false)
                throw new InvalidConstraintException("Stream must be seekable to produce a unique hash correctly");

            if (stream.Length <= 0)
                return string.Empty;

            var currentPosition = stream.Position;
            var first = (byte)stream.ReadByte();
            stream.Seek(currentPosition, SeekOrigin.Begin);

            var hashBytes = await OnGetHash(stream, token);

            currentPosition = stream.Position;
            stream.Seek(currentPosition - 1, SeekOrigin.Begin);
            var last = (byte)stream.ReadByte();

            token.ThrowIfCancellationRequested();
            return FinalizeHash(hashBytes, first, last, (ulong)stream.Length);
        }

        /// <inheritdoc />
        public async ValueTask<string> GetHash(Uri target,
                                               IFileSystemHandler fileSystemHandler,
                                               bool recursive = false,
                                               CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(target);

            if (!target.IsAbsoluteUri)
                throw new InvalidOperationException(nameof(target) + " must be an absolute path.");

            if (fileSystemHandler.IsFile(target))
            {
                using (var fileStream = fileSystemHandler.OpenRead(target))
                {
                    if (fileStream == null)
                        throw new NullReferenceException(nameof(IFileSystemHandler) + "." + nameof(IFileSystemHandler.OpenRead) + " return null from " + target);

                    return await GetHash(fileStream, token);
                }
            }

            var allFiles = fileSystemHandler.SearchFiles(target.LocalPath, "*", recursive);

            return await GetHash(allFiles, fileSystemHandler, token);
        }

        /// <inheritdoc />
        public async ValueTask<string> GetHash(IReadOnlyCollection<Uri> files, IFileSystemHandler fileSystemHandler, CancellationToken token = default)
        {
            var hashFilesHashages = files.OrderBy(f => f.LocalPath)
                                         .Select(f => GetHash(f, fileSystemHandler, false, token).AsTask())
                                         .ToArray();

            var results = await Task.WhenAll(hashFilesHashages);

            var stringBuilder = new StringBuilder(results.Length);

            foreach (var result in results)
                stringBuilder.Append(result);

            return await GetHash(stringBuilder.ToString(), Encoding.ASCII, token);
        }

        #region Tools

        /// <summary>
        /// Called when get data hash.
        /// </summary>
        /// <remarks>
        ///     ATTENTION : Result must not exceed <see cref="sizeof(ushort)"/>
        /// </remarks>
        protected virtual async ValueTask<byte[]> OnGetHash(Stream stream,
                                                            CancellationToken token = default)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (stream.Length == 0)
                return EnumerableHelper<byte>.ReadOnlyArray;

            using (var hasher = this._hashAlgorithmFactory())
            {
                byte[] results;

                if (stream.Length < 65536)
                {
                    results = hasher.ComputeHash(stream);
                    token.ThrowIfCancellationRequested();
                }
                else
                {
                    results = await hasher.ComputeHashAsync(stream, token);
                }

                token.ThrowIfCancellationRequested();
                return results;
            }
        }

        /// <summary>
        /// Finalizes the hash, to ensure unicity add first and last byte and byte length
        /// </summary>
        private string FinalizeHash(in ReadOnlySpan<byte> hash, in byte first, in byte last, ulong sourceSize)
        {
            // Array to store hash unique value : FIRST_SOURCE_BYTE + HASH + LAST_SOURCE_BYTE + HASH_LENGTH
            Span<byte> buf = stackalloc byte[1 + hash.Length + 1 + sizeof(ulong)];

            buf[0] = first;
            hash.CopyTo(buf.Slice(1));

            // Due to first byte added need to add 1
            buf[hash.Length + 1] = last;

            ReadOnlySpan<byte> lengthBytes  = BitConverter.GetBytes(sourceSize);
            
            // Due to first and last bytes added slide need to add 1 + 1
            lengthBytes.CopyTo(buf.Slice(hash.Length + 1 + 1));

            return Convert.ToBase64String(buf);
        }

        #endregion

        #endregion
    }
}
