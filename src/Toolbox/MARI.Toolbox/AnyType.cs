// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox
{
    using MARI.Toolbox.Models;

    using System.ComponentModel;
    using System.Runtime.Serialization;

    public interface IAnyType
    {
    }

    /// <summary>
    /// Define a type used as all accepted type
    /// </summary>
    [Serializable]
    [ImmutableObject(true)]
    public sealed class AnyType : IEquatable<AnyType>, IAnyType
    {
        #region Ctor

        /// <summary>
        /// Initializes the <see cref="NoneType"/> class.
        /// </summary>
        static AnyType()
        {
            Trait = typeof(AnyType);
            IAnyTypeTrait = typeof(IAnyType);
            AbstractTrait = Trait.GetAbstractType();
            Instance = new AnyType();
        }

        /// <summary>
        /// Prevents a default data of the <see cref="AnyType"/> class from being created.
        /// </summary>
        private AnyType()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the trait.
        /// </summary>
        public static Type Trait { get; }

        /// <summary>
        /// Gets the <see cref="IAnyType"/> trait.
        /// </summary>
        public static Type IAnyTypeTrait { get; }

        /// <summary>
        /// Gets the trait.
        /// </summary>
        public static AbstractType AbstractTrait { get; }

        /// <summary>
        /// Gets the data.
        /// </summary>
        public static AnyType Instance { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Create a container.
        /// </summary>
        public static IAnyTypeContainer CreateContainer(object data)
        {
            ArgumentNullException.ThrowIfNull(data);
            return CreateContainer(data, data.GetType());
        }

        /// <summary>
        /// Create a container.
        /// </summary>
        public static IAnyTypeContainer CreateContainer(object? data, Type dataType)
        {
            var containerType = typeof(AnyTypeContainer<>).MakeGenericType(dataType);
            return (IAnyTypeContainer)Activator.CreateInstance(containerType, new[] { data })!;
        }

        /// <summary>
        /// Determines whether <typeparamref name="TType"/> is equal to <see cref="NoneType"/>.
        /// </summary>
        public static bool IsEqualTo<TType>()
        {
            var type = typeof(TType);
            return type == Trait || typeof(TType) == IAnyTypeTrait;
        }

        /// <inheritdoc />
        public bool Equals(AnyType? other)
        {
            return other is not null;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is AnyType;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return 4242;
        }

        #endregion
    }
}
