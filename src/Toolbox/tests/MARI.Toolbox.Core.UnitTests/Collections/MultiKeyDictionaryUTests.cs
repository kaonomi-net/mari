// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Core.UnitTests.Collections
{
    using MARI.Toolbox.Collections;

    using NFluent;

    using System;
    using System.Linq;

    /// <summary>
    /// Test for <see cref="MultiKeyDictionary"/>
    /// </summary>
    public sealed class MultiKeyDictionaryUTests
    {
        #region Methods

        [Fact]
        public void MultiKeyDictionary_MultiSame_Keys()
        {
            var testKeys = new string[]
            {
                "Multi",
                "Multi String",
                "Multi String Keys",
                "Multi String Keys Value",
            };

            var dic = new MultiKeyDictionary<string, string>();

            int i = 0;
            foreach (var key in testKeys)
            {
                var parts = key.Split(' ');

                if (i % 2 == 0)
                    dic.Add(parts, key);
                else
                    dic[parts] = key;

                ++i;
            }

            Check.That(dic.GetItems().ToArray().Length).IsEqualTo(testKeys.Length);

            foreach (var key in testKeys)
            {
                var parts = key.Split(' ');

                bool addFailed = false;
                try
                {
                    dic.Add(parts, key);
                    addFailed = true;
                }
                catch (Exception ex)
                {

                }

                Check.WithCustomMessage("Couldn't add same key twice").That(addFailed).IsEqualTo(false);

                var found = dic.TryGetValue(parts, out var result);

                Check.That(found).IsTrue();
                Check.That(result).IsEqualTo(key);
            }
        }

        [Fact]
        public void MultiKeyDictionary_MultiSame_Iteration()
        {
            var testKeys = new string[]
            {
                "Multi",
                "Multi String",
                "Multi String Keys Item2",
                "Multi String Keys Item2 Item3",
                "Multi String Keys",
                "Multi String Keys Value",
                "Multi String Keys Value Val 3",
            };

            var dic = new MultiKeyDictionary<string, string>();

            int i = 0;
            foreach (var key in testKeys)
            {
                var parts = key.Split(' ');

                if (i % 2 == 0)
                    dic.Add(parts, key);
                else
                    dic[parts] = key;

                ++i;
            }

            foreach (var kVal in dic.GetItems())
            {
                Console.WriteLine(kVal);
            }

            var result = dic.GetItems().ToArray();

            Check.That(result.Length).IsEqualTo(testKeys.Length);

            var kv = dic.GetItems()
                        .Select(kv =>
                        {
                            var parts = kv.Value.Split(' ');

                            Check.That(parts).CountIs(kv.Key.Count).And.ContainsExactly(kv.Key);

                            return kv.Value;
                        })
                        .OrderBy(kv => kv)
                        .ToArray();

            Check.That(kv).CountIs(testKeys.Length).And.ContainsExactly(testKeys.OrderBy(k => k).ToArray());
        }

        [Fact]
        public void MultiKeyDictionary_Remove()
        {
            var testKeys = new string[]
            {
                "Multi",
                "Multi String",
                "Multi String Keys Item2",
                "Multi String Keys Item2 Item3",
                "Multi String Keys",
                "Multi String Keys Value",
                "Multi String Keys Value Val 3",
            };

            var dic = new MultiKeyDictionary<string, string>();

            foreach (var key in testKeys)
            {
                var parts = key.Split(' ');
                dic.Add(parts, key);
            }

            var arrays = dic.GetItems()
                            .OrderBy(o => o.Value)
                            .Select(kv => kv.Value)
                            .ToArray();

            Check.That(arrays).CountIs(testKeys.Length)
                              .And
                              .ContainsExactly(testKeys.OrderBy(k => k).ToArray());

            var removeIndx = Random.Shared.Next(0, testKeys.Length);

            var str = testKeys[removeIndx];
            var strParts = str.Split(' ');

            var removeResult = dic.Remove(strParts);
            Check.That(removeResult).IsTrue();

            var otherArrays = dic.GetItems()
                                 .OrderBy(o => o.Value)
                                 .Select(kv => kv.Value)
                                 .ToArray();

            var expect = testKeys.Select((k, i) => (k, i))
                                 .Where(kv => kv.i != removeIndx)
                                 .Select(kv => kv.k)
                                 .OrderBy(k => k).ToArray();

            Check.That(otherArrays).CountIs(expect.Length)
                                   .And
                                   .ContainsExactly(expect);

        }

        [Fact]
        public void MultiKeyDictionary_Clear()
        {
            var testKeys = new string[]
            {
                "Multi",
                "Multi String",
                "Multi String Keys Item2",
                "Multi String Keys Item2 Item3",
                "Multi String Keys",
                "Multi String Keys Value",
                "Multi String Keys Value Val 3",
            };

            var dic = new MultiKeyDictionary<string, string>();

            foreach (var key in testKeys)
            {
                var parts = key.Split(' ');
                dic.Add(parts, key);
            }

            var arrays = dic.GetItems()
                            .OrderBy(o => o.Value)
                            .Select(kv => kv.Value)
                            .ToArray();

            Check.That(arrays).CountIs(testKeys.Length)
                              .And
                              .ContainsExactly(testKeys.OrderBy(k => k).ToArray());

            dic.Clear();

            var afterClear = dic.GetItems()
                                .OrderBy(o => o.Value)
                                .Select(kv => kv.Value)
                                .ToArray();

            Check.That(afterClear).CountIs(0);
        }

        #endregion
    }
}
