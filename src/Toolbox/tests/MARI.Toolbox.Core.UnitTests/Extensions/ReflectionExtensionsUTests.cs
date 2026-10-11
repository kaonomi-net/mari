// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Core.UnitTests.Extensions
{
    using NFluent;

    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Test for <see cref="ReflectionImplExtensions"/>
    /// </summary>
    public sealed class ReflectionExtensionsUTests
    {
        private static readonly MethodInfo s_genericMethod;

        static ReflectionExtensionsUTests()
        {
            Expression<Func<int, string, int>> func = (i, s) => Method(i, s);

            s_genericMethod = ((MethodCallExpression)func.Body).Method.GetGenericMethodDefinition();
        }

        /// <summary>
        /// Ensure <see cref="ReflectionImplExtensions.MakeGenericMethodWithCache(MethodInfo, Type[])"/> returns the same cached instance for the same generic arguments
        /// </summary>
        [Fact]
        public void MakeGenericMethodWithCache_Same_Arguments_Return_Cached_Instance()
        {
            var keys = new[] { typeof(double), typeof(string) };

            var method = s_genericMethod.MakeGenericMethodWithCache(keys);
            var method2 = s_genericMethod.MakeGenericMethodWithCache(new[] { typeof(double), typeof(string) });

            Check.That(method).IsEqualTo(s_genericMethod.MakeGenericMethod(keys));
            Check.That(RuntimeHelpers.GetHashCode(method2)).IsEqualTo(RuntimeHelpers.GetHashCode(method));
            Check.That(object.ReferenceEquals(method, method2)).IsTrue();
        }

        /// <summary>
        /// Ensure <see cref="ReflectionImplExtensions.MakeGenericMethodWithCache(MethodInfo, Type[])"/> distinguishes different generic arguments
        /// </summary>
        [Fact]
        public void MakeGenericMethodWithCache_Different_Arguments_Return_Different_Instance()
        {
            var method = s_genericMethod.MakeGenericMethodWithCache(typeof(double), typeof(string));
            var other = s_genericMethod.MakeGenericMethodWithCache(typeof(int), typeof(string));

            Check.That(other).IsEqualTo(s_genericMethod.MakeGenericMethod(typeof(int), typeof(string)));
            Check.That(object.ReferenceEquals(method, other)).IsFalse();
        }

        /// <summary>
        /// Ensure <see cref="ReflectionImplExtensions.MakeGenericTypeWithCache(Type, Type[])"/> returns the expected type and caches it
        /// </summary>
        [Fact]
        public void MakeGenericTypeWithCache_Same_Arguments_Return_Cached_Instance()
        {
            var type = typeof(Dictionary<,>).MakeGenericTypeWithCache(typeof(string), typeof(Guid));
            var type2 = typeof(Dictionary<,>).MakeGenericTypeWithCache(typeof(string), typeof(Guid));
            var other = typeof(Dictionary<,>).MakeGenericTypeWithCache(typeof(Guid), typeof(string));

            Check.That(type).IsEqualTo(typeof(Dictionary<string, Guid>));
            Check.That(object.ReferenceEquals(type, type2)).IsTrue();
            Check.That(other).IsEqualTo(typeof(Dictionary<Guid, string>));
        }

        private static TValue Method<TValue, Tvalue2>(TValue value, Tvalue2 str)
        {
            return value;
        }
    }
}
