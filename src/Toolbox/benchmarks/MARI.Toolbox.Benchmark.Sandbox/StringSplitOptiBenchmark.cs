// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Benchmark.Sandbox
{
    using BenchmarkDotNet.Attributes;

    using MARI.Toolbox.Models;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    [MemoryDiagnoser(true)]
    //[SimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput)]//, iterationCount: 100, invocationCount: 100_000)]
    public class StringSplitOptiBenchmark
    {
        private StringIndexedContext _separatorCtx;
        private string[] _separators;
        private string _str;

        [GlobalSetup]
        public void StringSplitOptiBenchmark_Setups()
        {
            _str = new string(Enumerable.Range(0, 10_000)
                                         .SelectMany(i => Guid.NewGuid().ToString())
                                         .ToArray());

            _separators = new string[]
            {
                "-", "42", "A", "421" ,"AB", "AZ", "AZE"
            };

            _separatorCtx = StringIndexedContext.Create(_separators, EnumerableHelper<string>.ReadOnlyArray);
        }

        // Split each part
        [Benchmark]
        public string[] ClassicSplit()
        {
            return _str.Split(_separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        [Benchmark]
        public IReadOnlyList<string> OptiSplit()
        {
            return _str.OptiSplit(Models.StringIncludeSeparatorMode.Isolated,
                                   StringComparison.Ordinal,
                                   StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries,
                                   _separators);
        }

        [Benchmark]
        public IReadOnlyList<string> OptiSplitCtx()
        {
            return _str.OptiSplit(Models.StringIncludeSeparatorMode.Isolated,
                                   StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries,
                                   _separatorCtx);
        }

        [Benchmark]
        public IReadOnlyList<string> ClassicSplitAttached()
        {
            var results = _str.Split(_separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var final = new List<string>((int)((float)results.Length / 2.0f) + 1);

            for (var i = 0; i + 1 < results.Length; i += 2)
            {
                final.Add(results[i] + results[i + 1]);
            }

            return final;
        }

        // Attached
        [Benchmark]
        public IReadOnlyList<string> OptiSplitAttached()
        {
            return _str.OptiSplit(Models.StringIncludeSeparatorMode.AttachedToPrevious,
                                   StringComparison.Ordinal,
                                   StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries,
                                   _separators);
        }

        [Benchmark]
        public IReadOnlyList<string> OptiSplitCtxAttached()
        {
            return _str.OptiSplit(Models.StringIncludeSeparatorMode.AttachedToPrevious,
                                   StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries,
                                   _separatorCtx);
        }
    }
}
