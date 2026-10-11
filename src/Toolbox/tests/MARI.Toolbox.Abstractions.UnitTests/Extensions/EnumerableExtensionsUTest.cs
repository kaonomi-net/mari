// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.UnitTests.Extensions
{
    using Newtonsoft.Json.Linq;

    using NFluent;

    using NSubstitute;

    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using Xunit;

    /// <summary>
    /// Test for <see cref="EnumerableExtensions"/>
    /// </summary>
    public sealed class EnumerableExtensionsUTest
    {
        private delegate Dictionary<string, Dictionary<int, Tuple<double, TArg, List<TValue>>>> MapTreeGeneric<TArg, TValue>();

        /// <summary>
        /// Enumerables the try get value inline.
        /// </summary>
        [Fact]
        public void Enumerable_TryGetValueInline()
        {
            var dic = new Dictionary<string, int>()
            {
                { "value", 42 }
            };

            // Not Founded without default value
            var defaultNotFounded = dic.TryGetValueInline("NotFounded", out var notFounded);

            Check.That(defaultNotFounded).IsEqualTo(0);
            Check.That(notFounded).IsFalse();

            // Founded
            var foundedValue = dic.TryGetValueInline("value", out var founded, 21);

            Check.That(foundedValue).IsEqualTo(42);
            Check.That(founded).IsTrue();

            // Not Founded without default value
            var defaultNotFoundedWithDefault = dic.TryGetValueInline("NotFounded", out var notFoundedWithDefault, 12);

            Check.That(defaultNotFoundedWithDefault).IsEqualTo(12);
            Check.That(notFoundedWithDefault).IsFalse();
        }

        /// <summary>
        /// Searches the in type tree.
        /// </summary>
        [Fact]
        public void Search_In_TypeTree()
        {
            var type = typeof(MapTreeGeneric<,>).GetMethod("Invoke")!.ReturnType;

            var args = type.SearchInTree(t => t.IsGenericType ? t.GetGenericArguments() : EnumerableHelper<Type>.ReadOnlyArray,
                                         t => t.IsGenericMethodParameter || t.IsGenericTypeParameter);

            Check.That(args).IsNotNull().And.CountIs(2);

            var delegateType = typeof(MapTreeGeneric<,>);
            var garg0 = delegateType.GetGenericArguments().First();
            var garg1 = delegateType.GetGenericArguments().Last();

            var arg = args.First();
            Check.That(arg.Node).IsNotNull().And.IsEqualTo(garg0);
            Check.That(arg.Path.Parts).IsNotNull().And.CountIs(4).And.ContainsExactly(0, 1, 1, 1);

            var value = args.Last();
            Check.That(value.Node).IsNotNull().And.IsEqualTo(garg1);
            Check.That(value.Path.Parts).IsNotNull().And.CountIs(5).And.ContainsExactly(0, 1, 1, 2, 0);
        }

        /// <summary>
        /// Searches the in type tree.
        /// </summary>
        [Fact]
        public void Search_In_TypeTree_Resolve()
        {
            var type = typeof(MapTreeGeneric<,>).GetMethod("Invoke")!.ReturnType;

            var args = type.SearchInTree(t => t.IsGenericType ? t.GetGenericArguments() : EnumerableHelper<Type>.ReadOnlyArray,
                                         t => t.IsGenericMethodParameter || t.IsGenericTypeParameter);
            Check.That(args).IsNotNull().And.CountIs(2);

            var finalType = typeof(Dictionary<string, Dictionary<int, Tuple<double, EnumerableExtensionsUTest, List<Uri>>>>);

            var arg = args.First();
            var value = args.Last();

            var resolvArg = arg.Path.FoundNodeByPath(finalType, t => t.IsGenericType ? t.GetGenericArguments() : EnumerableHelper<Type>.ReadOnlyArray);
            var resolvVal = value.Path.FoundNodeByPath(finalType, t => t.IsGenericType ? t.GetGenericArguments() : EnumerableHelper<Type>.ReadOnlyArray);

            Check.That(resolvArg).IsNotNull().And.IsEqualTo(typeof(EnumerableExtensionsUTest));
            Check.That(resolvVal).IsNotNull().And.IsEqualTo(typeof(Uri));
        }

        /// <summary>
        /// Searches the in type tree.
        /// </summary>
        [Fact]
        public void Search_In_TypeTree_Resolve_Root()
        {
            var type = typeof(Tuple<,>);

            var args = type.SearchInTree(t => t.IsGenericType ? t.GetGenericArguments() : EnumerableHelper<Type>.ReadOnlyArray,
                                         t => t.IsGenericMethodParameter || t.IsGenericTypeParameter);

            Check.That(args).IsNotNull().And.CountIs(2);

            var finalType = typeof(Tuple<EnumerableExtensionsUTest, Uri>);

            var arg = args.First();
            var value = args.Last();

            var resolvArg = arg.Path.FoundNodeByPath(finalType, t => t.IsGenericType ? t.GetGenericArguments() : EnumerableHelper<Type>.ReadOnlyArray);
            var resolvVal = value.Path.FoundNodeByPath(finalType, t => t.IsGenericType ? t.GetGenericArguments() : EnumerableHelper<Type>.ReadOnlyArray);

            Check.That(resolvArg).IsNotNull().And.IsEqualTo(typeof(EnumerableExtensionsUTest));
            Check.That(resolvVal).IsNotNull().And.IsEqualTo(typeof(Uri));
        }

        /// <summary>
        /// Searches the in type tree.
        /// </summary>
        [Fact]
        public void Search_In_NO_TypeTree_Root()
        {
            var type = typeof(string);

            var args = type.SearchInTree(t => t.IsGenericType ? t.GetGenericArguments() : EnumerableHelper<Type>.ReadOnlyArray,
                                         t => true);

            Check.That(args).IsNotNull().And.CountIs(1);

            var finalType = typeof(Uri);

            var arg = args.First();

            var resolvArg = arg.Path.FoundNodeByPath(finalType, t => t.IsGenericType ? t.GetGenericArguments() : EnumerableHelper<Type>.ReadOnlyArray);

            Check.That(resolvArg).IsNotNull().And.IsEqualTo(typeof(Uri));
        }
    }
}
