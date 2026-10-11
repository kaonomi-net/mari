// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Enums
{
    /// <summary>
    /// System storage type
    /// </summary>
    [Flags]
    public enum StorageTypeEnum : ushort
    {
        None = 0,

        /// <summary>
        /// Define a storage used when no specific if definied
        /// </summary>
        Default = 0x0001,

        /// <summary>
        /// Define a storage used by mari system vgrain.
        /// </summary>
        MARI = 0x0002,

        /// <summary>
        /// Define a storage used to store reminder information.
        /// </summary>
        Reminders = 0x0004,

        /// <summary>
        /// Define a storage used to store reminder information.
        /// </summary>
        Repositories = 0x0008,

        /// <summary>
        /// The dynamic definition storage
        /// </summary>
        DynamicDefinition = 0x0010,

        /// <summary>
        /// Define a storage used to store mari high system information.
        /// </summary>
        MARIAdmin = 0x8000,

        All = StorageTypeEnum.MARI | 
              StorageTypeEnum.Reminders | 
              StorageTypeEnum.Default |
              StorageTypeEnum.Repositories |
              StorageTypeEnum.DynamicDefinition |
              StorageTypeEnum.MARIAdmin
    }
}
