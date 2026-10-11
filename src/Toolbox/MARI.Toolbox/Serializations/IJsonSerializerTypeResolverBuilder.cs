// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Serializations
{
    using System.Linq.Expressions;

    /// <summary>
    /// Used to build a type resolver to solve polymorphism
    /// </summary>
    public interface IJsonSerializerTypeResolverBuilder<TType>
    {
        /// <summary>
        /// Apply a predicate to choose the specific type <typeparamref name="TResult"/>
        /// </summary>
        IJsonSerializerTypeResolverBuilder<TType, TPropValue> Where<TPropValue>(Expression<Func<TType, TPropValue>> prop, TPropValue value, IEqualityComparer<TPropValue>? comparer = null);

        /// <summary>
        /// End the current build session a turn back to root for a new type mapping
        /// </summary>
        IJsonSerializerConverterBuilder Done { get; }
    }

    /// <summary>
    /// Define What to apply when a condition have been fullfill
    /// </summary>
    public interface IJsonSerializerTypeResolverBuilder<TType, TPropType>
    {
        /// <summary>
        /// Applies the <typeparamref name="TSubType"/> as concret type to use
        /// </summary>
        IJsonSerializerTypeResolverBuilder<TType> ApplyType<TSubType>()
            where TSubType : TType;
    }

}
