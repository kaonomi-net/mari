// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.UnitTests.Comparers
{
    using AutoFixture;

    using MARI.Toolbox.Abstractions.Comparers;

    using NFluent;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Xunit;

    /// <summary>
    /// Test for <see cref="UriComparer"/>
    /// </summary>
    public class UriComparerUTest
    {
        [Fact]
        public void Compare_Standard()
        {
            var fixture = new Fixture();
            var testUri = fixture.Create<Uri>();

            var defaultComparer = new UriComparer(false, StringComparison.Ordinal);

            Check.That(defaultComparer.GetHashCode(testUri)).IsEqualTo(testUri.GetHashCode());
            Check.That(defaultComparer.Equals(testUri, testUri)).IsTrue();
        }

        [Fact]
        public void Compare_MultiCase()
        {
            var fixture = new Fixture();

            var root = fixture.Create<Uri>().OriginalString;
            var fragment = fixture.Create<string>();

            var testRootUri = new Uri(root);
            var testUri = new Uri(root + "#" + fragment);
            var upperTestUri = new Uri(root + "#" + fragment.ToUpper());

            var ignoreCaseComparer = new UriComparer(true, StringComparison.OrdinalIgnoreCase);

            Check.That(ignoreCaseComparer.GetHashCode(upperTestUri)).IsEqualTo(ignoreCaseComparer.GetHashCode(testUri));
            Check.That(ignoreCaseComparer.Equals(upperTestUri, testUri)).IsTrue();

            Check.That(ignoreCaseComparer.GetHashCode(testRootUri)).IsNotEqualTo(ignoreCaseComparer.GetHashCode(testUri));
            Check.That(ignoreCaseComparer.Equals(testRootUri, testUri)).IsFalse();

            var defaultComparer = new UriComparer(true, StringComparison.Ordinal);

            Check.That(defaultComparer.GetHashCode(upperTestUri)).IsEqualTo(defaultComparer.GetHashCode(testUri));
            Check.That(defaultComparer.Equals(upperTestUri, testUri)).IsFalse();

            Check.That(ignoreCaseComparer.GetHashCode(testRootUri)).IsNotEqualTo(ignoreCaseComparer.GetHashCode(testUri));
            Check.That(ignoreCaseComparer.Equals(testRootUri, testUri)).IsFalse();

        }
    }
}
