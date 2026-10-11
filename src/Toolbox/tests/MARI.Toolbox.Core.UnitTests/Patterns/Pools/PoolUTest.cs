// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Core.UnitTests.Patterns.Pools
{
    using MARI.Toolbox.Patterns.Pools;

    using NFluent;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Test for <see cref="Pool<>"/>
    /// </summary>
    public sealed class PoolUTest
    {
        public sealed class TestPoolItem : PoolBaseItem
        {
            /// <inheritdoc />
            protected override void OnCleanUp()
            {
            }
        }

        /// <summary>
        /// Try recycling on not thread safe pool
        /// </summary>
        [Fact]
        public void UnSafe_Pool()
        {
            var pool = new Pool<TestPoolItem>(12);

            var items = new HashSet<TestPoolItem>();

            for (int i = 0; i < 100; i++)
            {
                var it = pool.GetItem();

                items.Add(it);

                if (i > 5)
                    it.Release();
            }

            Check.That(items.Count).IsEqualTo(7);
        }

        /// <summary>
        /// Thread safe pool test: deterministic invariants (no scheduling dependent outcome)
        ///   1) Items handed out concurrently are never shared (all distinct)
        ///   2) Once released, the items are reused by the next concurrent requests
        /// </summary>
        [Fact]
        public async Task ThreadSafe_Pool()
        {
            const int TEST_SIZE = 120;

            using (var pool = new ThreadSafePool<TestPoolItem>(500))
            {
                // 1) Concurrent requests without release: every request gets its own item
                var firstRound = await Task.WhenAll(Enumerable.Range(0, TEST_SIZE)
                                                              .Select(_ => Task.Run(() => pool.GetItem())));

                Check.That(firstRound.Distinct().Count()).IsEqualTo(TEST_SIZE);

                // Release everything: the pool keeps them (capacity 500 > TEST_SIZE)
                foreach (var item in firstRound)
                    item.Release();

                // 2) Concurrent requests after release: all items come back from the pool
                var secondRound = await Task.WhenAll(Enumerable.Range(0, TEST_SIZE)
                                                               .Select(_ => Task.Run(() => pool.GetItem())));

                Check.That(secondRound.Distinct().Count()).IsEqualTo(TEST_SIZE);
                Check.That(secondRound.All(i => firstRound.Contains(i))).IsTrue();
            }
        }
    }
}
