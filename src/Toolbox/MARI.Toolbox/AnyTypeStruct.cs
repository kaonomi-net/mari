// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox
{
    using System.ComponentModel;

    /// <summary>
    /// Define a type used as Any type (All type accepted)
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public readonly struct AnyTypeStruct
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="AnyType"/> class.
        /// </summary>
        static AnyTypeStruct()
        {
            Trait = typeof(AnyTypeStruct);
            Instance = new AnyTypeStruct();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the trait.
        /// </summary>
        public static Type Trait { get; }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static AnyTypeStruct Instance { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether <typeparamref name="TType"/> is equal to <see cref="AnyType"/>.
        /// </summary>
        public static bool IsEqualTo<TType>()
            where TType : struct
        {
            return typeof(TType) == Trait;
        }

        #endregion
    }
}
