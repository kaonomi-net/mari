// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Benchmark.Sandbox
{
    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Engines;

    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    [MemoryDiagnoser(true)]
    [SimpleJob(RunStrategy.Throughput, iterationCount: 100, invocationCount: 100_000)]
    public class ReflectionCacheBenchmark
    {
        private static readonly MethodInfo s_genericMethod;
        private static readonly Type s_multipleTypeGeneric;

        private static readonly Type[] s_argType;
        private static readonly Type[] s_argOtherType;

        static ReflectionCacheBenchmark()
        {
            Expression<Func<int, string, int>> func = (i, s) => Method(i, s);

            s_genericMethod = ((MethodCallExpression)func.Body).Method.GetGenericMethodDefinition();

            s_multipleTypeGeneric = typeof(IDictionary<,>);

            s_argType = new []
            {
                typeof(string), 
                typeof(double)
            };

            s_argOtherType = new []
            {
                typeof(byte),
                typeof(long)
            };
        }

        [Benchmark]
        public MethodInfo MakeNewReflectionMethod()
        {
            return s_genericMethod.MakeGenericMethod(s_argType);
        }

        [Benchmark]
        public MethodInfo MakeNewReflectionMethodWithCache()
        {
            return s_genericMethod.MakeGenericMethodWithCache(s_argOtherType);
        }

        [Benchmark]
        public Type MakeNewReflectionType()
        {
            return s_multipleTypeGeneric.MakeGenericType(s_argOtherType);
        }

        [Benchmark]
        public Type MakeNewReflectionTypeWithCache()
        {
            return s_multipleTypeGeneric.MakeGenericTypeWithCache(s_argType);
        }

        private static TValue Method<TValue, Tvalue2>(TValue value, Tvalue2 str)
        {
            return value;
        }
    }
}
