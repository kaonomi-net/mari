// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.UnitTests.Extensions
{
    using NFluent;

    using System;
    using System.Reflection;

    using Xunit;

    /// <summary>
    /// Test for <see cref="ReflectionExtensions"/>
    /// </summary>
    public class ReflectionExtensionsUTest
    {
        [Fact]
        public void Generate_Unique_Id()
        {
            var mthod = typeof(object).GetMethod(nameof(object.GetHashCode));
            var otherMthod = typeof(object).GetMethod(nameof(object.GetHashCode));
            var intMthod = typeof(int).GetMethod(nameof(object.GetHashCode));

            var unique = ReflectionExtensions.GetUniqueId(mthod);
            var otherUnique = ReflectionExtensions.GetUniqueId(otherMthod);
            var intUnique = ReflectionExtensions.GetUniqueId(intMthod);

            Check.That(unique).IsNotNull().And.IsNotEmpty().And.IsEqualTo(otherUnique);

            Check.That(unique).IsNotNull().And.IsNotEmpty().And.IsNotEqualTo(intUnique);
            unique = unique.Substring(unique.IndexOf('_'));
            intUnique = intUnique.Substring(intUnique.IndexOf('_'));
            Check.That(unique).IsNotNull().And.IsNotEmpty().And.IsEqualTo(intUnique);
        }
    }
}
