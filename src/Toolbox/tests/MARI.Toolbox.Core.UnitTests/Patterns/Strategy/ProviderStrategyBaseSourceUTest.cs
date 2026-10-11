// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Core.UnitTests.Patterns.Strategy
{
    using MARI.Toolbox.Patterns.Strategy;

    using NFluent;

    using NSubstitute;

    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Test for <see cref="ProviderStrategyBaseSource{TKey, TSource}"/>
    /// </summary>
    public sealed class ProviderStrategyBaseSourceUTest
    {
        #region Nested

        private sealed class TestProviderStrategySource : ProviderStrategyBaseSource<string, Guid>
        {
            private readonly Func<IEnumerable<(Guid key, string value)>>? _update;

            /// <summary>
            /// Initializes a new instance of the <see cref="TestProviderStrategySource"/> class.
            /// </summary>
            public TestProviderStrategySource(IServiceProvider serviceProvider,
                                              IEnumerable<(Guid key, string value)>? initValues,
                                              Func<IEnumerable<(Guid key, string value)>>? update,
                                              bool supportFallback)
                : base(serviceProvider, initValues, supportFallback: supportFallback)
            {
                this._update = update;
            }

            protected override Task FallbackOdRetryFailedAsync(CancellationToken token)
            {
                if (this._update is not null)
                {
                    var nextData = this._update();

                    foreach (var d in nextData ?? EnumerableHelper<(Guid key, string value)>.ReadOnlyArray)
                        base.SafeAddOrReplace(d.key, d.value);
                }
                return base.FallbackOdRetryFailedAsync(token);
            }
        }

        #endregion

        #region Methods

        [Fact]
        public async Task ProviderStrategyBaseSource_InitValue()
        {
            var serviceProvider = Substitute.For<IServiceProvider>();

            var sampleData = new[]
            {
                (Guid.NewGuid(), Guid.NewGuid().ToString()),
                (Guid.NewGuid(), Guid.NewGuid().ToString()),
                (Guid.NewGuid(), Guid.NewGuid().ToString()),
            };

            var testSource = new TestProviderStrategySource(serviceProvider, sampleData, null, true);

            foreach (var data in sampleData)
            {
                var result = await testSource.TryGetDataAsync(data.Item1, default);

                Check.That(result.Success).IsTrue();
                Check.That(result.Result).IsNotNull().And.IsNotEmpty().And.IsEqualTo(data.Item2);
            }
        }

        [Fact]
        public async Task ProviderStrategyBaseSource_Retry()
        {
            var serviceProvider = Substitute.For<IServiceProvider>();

            var sampleData = new[]
            {
                (Guid.NewGuid(), Guid.NewGuid().ToString()),
                (Guid.NewGuid(), Guid.NewGuid().ToString()),
                (Guid.NewGuid(), Guid.NewGuid().ToString()),
            };

            var updateCounter = 0;

            // Try No fallback without update
            var testSourceNoFallback = new TestProviderStrategySource(serviceProvider, null, () =>
            {
                updateCounter++;
                return EnumerableHelper<(Guid key, string value)>.Enumerable;
            }, false);

            Check.That(updateCounter).IsEqualTo(0);

            var result = await testSourceNoFallback.TryGetDataAsync(Guid.NewGuid(), default);

            // No Fallback
            Check.That(updateCounter).IsEqualTo(0);
            Check.That(result.Success).IsFalse();

            updateCounter = 0;

            Check.That(updateCounter).IsEqualTo(0);

            // Try fallback without update
            var testSource = new TestProviderStrategySource(serviceProvider, null, () =>
            {
                updateCounter++;
                return EnumerableHelper<(Guid key, string value)>.Enumerable;
            }, true);

            Check.That(updateCounter).IsEqualTo(0);

            result = await testSource.TryGetDataAsync(Guid.NewGuid(), default);

            Check.That(updateCounter).IsEqualTo(1);
            Check.That(result.Success).IsFalse();

            updateCounter = 0;

            Check.That(updateCounter).IsEqualTo(0);

            // Try fallback with update
            var testSourceWithFallback = new TestProviderStrategySource(serviceProvider, null, () =>
            {
                updateCounter++;
                return sampleData;
            }, true);

            foreach (var data in sampleData)
            {
                result = await testSourceWithFallback.TryGetDataAsync(data.Item1, default);

                Check.That(result.Success).IsTrue();
                Check.That(result.Result).IsNotNull().And.IsNotEmpty().And.IsEqualTo(data.Item2);
            }
         
            Check.That(updateCounter).IsEqualTo(1);
        }

        #endregion
    }
}
