// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Core.UnitTests.Services
{
    using MARI.Toolbox.Abstractions.Services;

    using NFluent;

    using System;
    using System.Data;
    using System.Security.Cryptography;

    /// <summary>
    /// Base class of unit test of all the hash service implementation
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    public abstract class HashBaseServiceUTest<TService>
        where TService : class, IHashService
    {
        #region Fields

        private readonly Func<HashAlgorithm> _funcAlgo;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="HashBaseServiceUTest{TService}"/> class.
        /// </summary>
        protected HashBaseServiceUTest(Func<HashAlgorithm> funcAlgo)
        {
            this._funcAlgo = funcAlgo;
        }

        #endregion

        #region Nested

        private sealed class CantSeekStream : Stream
        {
            public override bool CanRead
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return false;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override long Length
            {
                get
                {
                    return 42;
                }
            }

            public override long Position
            {
                get
                {
                    return 0;
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Test is null is pass as byte[]
        /// </summary>
        [Fact]
        public async Task HashService_Null()
        {
            var inst = CreateNew();
            try
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable expected type.
                Check.ThatCode(async () => await inst.GetHash((byte[])null)).Throws<ArgumentNullException>();
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable expected type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }
            finally
            {
                if (inst is IDisposable disposable)
                    disposable.Dispose();
                else if (inst is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync();
            }
        }

        /// <summary>
        /// Test if an empty stream is pass
        /// </summary>
        [Fact]
        public async Task HashService_Empty()
        {
            var inst = CreateNew();
            try
            {
                var hash = await inst.GetHash(EnumerableHelper<byte>.ReadOnlyArray);
                Check.That(hash).IsNotNull().And.IsEmpty();
            }
            finally
            {
                if (inst is IDisposable disposable)
                    disposable.Dispose();
                else if (inst is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync();
            }
        }

        [Fact]
        public Task HashService_Small()
        {
            return HashService_Test_Result(42);
        }

        [Fact]
        public Task HashService_Medium()
        {
            return HashService_Test_Result(short.MaxValue);
        }

        [Fact]
        public Task HashService_Big()
        {
            return HashService_Test_Result(100_000);
        }

        [Fact]
        public async Task HashService_Cant_Seek()
        {
            var inst = CreateNew();
            try
            {
                using (var stream = new CantSeekStream())
                {
                    Check.ThatCode(async () => await inst.GetHash(stream)).Throws<InvalidConstraintException>();
                }
            }
            finally
            {
                if (inst is IDisposable disposable)
                    disposable.Dispose();
                else if (inst is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync();
            }
        }

        #region Tools

        /// <summary>
        /// Hashes the service test result.
        /// </summary>
        private async Task HashService_Test_Result(int size)
        {
            var inst = CreateNew();
            try
            {

                var bytesToHash = CreateByteSource(size);

                var first = bytesToHash[0];
                var last = bytesToHash[^1];

                using (var stream = new MemoryStream(bytesToHash))
                {
                    var simpleHash = string.Empty;
                    var expected = string.Empty;
                    using (var baseHasher = this._funcAlgo())
                    {
                        var basicHash = baseHasher.ComputeHash(stream);
                        simpleHash = Convert.ToBase64String(basicHash);
                        var concatArray = first.AsEnumerable()
                                               .Concat(basicHash)
                                               .Concat(last.AsEnumerable())
                                               .Concat(BitConverter.GetBytes((ulong)size))
                                               .ToArray();

                        expected = Convert.ToBase64String(concatArray);
                    }

                    stream.Seek(0, SeekOrigin.Begin);

                    var resultHash = await inst.GetHash(stream);

                    Check.That(resultHash).IsNotNull().And
                                          .IsNotEmpty().And
                                          .IsNotEqualTo(simpleHash).And
                                          .IsEqualTo(expected);
                }
            }
            finally
            {
                if (inst is IDisposable disposable)
                    disposable.Dispose();
                else if (inst is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync();
            }
        }

        /// <summary>
        /// Creates the byte source.
        /// </summary>
        protected virtual byte[] CreateByteSource(int size)
        {
            var bytes = new byte[size];
            Random.Shared.NextBytes(bytes);
            return bytes;
        }

        /// <summary>
        /// Creates a new <typeparamref name="TService"/> to test
        /// </summary>
        protected virtual TService CreateNew()
        {
            return Activator.CreateInstance<TService>();
        }

        #endregion

        #endregion
    }
}
