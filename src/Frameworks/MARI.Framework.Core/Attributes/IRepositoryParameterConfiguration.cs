// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Attributes
{
    public interface IRepositoryParameterConfiguration
    {
        #region Properties

        /// <summary>
        /// Gets the name of the storage.
        /// </summary>
        string StorageName { get; }

        /// <summary>
        /// Gets the name of the state.
        /// </summary>
        string? ConfigurationName { get; }

        #endregion
    }
}
