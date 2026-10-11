// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Patterns.Messenger
{
    /// <summary>
    /// Category helper often used
    /// </summary>
    public static class MessageCategories
    {
        /// <summary>
        /// Used when an item is added to a container
        /// </summary>
        public const string Added = "Added";

        /// <summary>
        /// Used when an item is removed from container
        /// </summary>
        public const string Removed = "Removed";

        /// <summary>
        /// Used when a new item is created
        /// </summary>
        public const string Created = "Created";

        /// <summary>
        /// Used when an item is updated
        /// </summary>
        public const string Updated = "Updated";

        /// <summary>
        /// Used when an item is fully deleted
        /// </summary>
        public const string Deleted = "Deleted";
    }
}
