// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Repositories
{
    using MARI.Toolbox.Models;

    using Orleans.Concurrency;

    /// <summary>
    /// In memory storage grain used to serialize in binary data in the cluster memory
    /// Grain Id must be a compound (index+StorageName). the age name will be used to contact the correct registry
    /// </summary>
    public interface IMemoryStorageRepositoryGrain<TKey> : IGrainWithIntegerCompoundKey
        where TKey : notnull, IEquatable<TKey>
    {
        /// <summary>
        /// Writes the data.
        /// </summary>
        Task<bool> WriteDataAsync(TKey key,
                                  object? entityKey,
                                  bool insertIfNew,
                                  string stateName,
                                  ReadOnlyMemory<byte> data,
                                  AbstractType entityType,
                                  IReadOnlyCollection<AbstractType> parentTypes);

        /// <summary>
        /// Read data bytes
        /// </summary>
        [ReadOnly]
        Task<ReadOnlyMemory<byte>?> ReadDataAsync(TKey key);

        /// <summary>
        /// Deletes the data
        /// </summary>
        Task<bool> DeleteDataAsync(string stateName, TKey key, object? entityKey);
    }

    /// <summary>
    /// Backward compatibility extensions of <see cref="IMemoryStorageRepositoryGrain{TKey}"/>
    /// </summary>
    /// <remarks>
    ///     Implemented as extension (and not as default interface method) to prevent orleans to generate a remote call for it
    /// </remarks>
    public static class MemoryStorageRepositoryGrainCompatibilityExtensions
    {
        /// <summary>
        /// Writes the data.
        /// </summary>
        [Obsolete("Misspelled, use " + nameof(IMemoryStorageRepositoryGrain<int>.WriteDataAsync) + " instead.")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static Task<bool> WriteDatAsync<TKey>(this IMemoryStorageRepositoryGrain<TKey> grain,
                                                     TKey key,
                                                     object? entityKey,
                                                     bool insertIfNew,
                                                     string stateName,
                                                     ReadOnlyMemory<byte> data,
                                                     AbstractType entityType,
                                                     IReadOnlyCollection<AbstractType> parentTypes)
            where TKey : notnull, IEquatable<TKey>
        {
            ArgumentNullException.ThrowIfNull(grain);
            return grain.WriteDataAsync(key, entityKey, insertIfNew, stateName, data, entityType, parentTypes);
        }
    }
}
