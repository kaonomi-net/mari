// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.Supports
{
    using System.Threading.Tasks;

    /// <summary>
    /// Define an instance that support initialization
    /// </summary>
    public interface ISupportInitialization
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance is initializing.
        /// </summary>
        bool IsInitializing { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        bool IsInitialized { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the instance
        /// </summary>
        ValueTask InitializationAsync(CancellationToken token = default);

        #endregion
    }

    /// <summary>
    /// Define an instance that support initialization
    /// </summary>
    public interface ISupportInitialization<TState> : ISupportInitialization
    {
        #region Methods

        /// <summary>
        /// Initialize the instance
        /// </summary>
        ValueTask InitializationAsync(TState? initializationState, CancellationToken token = default);

        #endregion
    }
}
