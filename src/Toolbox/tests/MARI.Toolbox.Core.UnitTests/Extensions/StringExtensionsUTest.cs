// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.UnitTests.Extensions
{
    using MARI.Toolbox.Models;

    using NFluent;

    using System;
    using System.Collections;

    /// <summary>
    /// Test for <see cref="StringExtensions"/>
    /// </summary>
    public sealed class StringExtensionsUTest
    {
        [Theory]
        [InlineData("List", false, StringSplitOptions.RemoveEmptyEntries, new[] { "<<", ">>" }, "List")]
        [InlineData("List<<int>>", false, StringSplitOptions.RemoveEmptyEntries, new[] { "<<", ">>" }, "List", "int")]
        [InlineData("List<<int>>", true, StringSplitOptions.RemoveEmptyEntries, new[] { "<<", ">>" }, "List", "<<", "int", ">>")]
        [InlineData("<<List<<int>>", true, StringSplitOptions.RemoveEmptyEntries, new[] { "<<", ">>" }, "<<", "List", "<<", "int", ">>")]
        [InlineData("    List`3<<int, double, List<<string, double, float>>   >>",
                    true,
                    StringSplitOptions.RemoveEmptyEntries,
                    new[] { "<<", ">>", ",", " " },
                    "List`3", "<<", "int", ",", "double", ",", "List", "<<", "string", ",", "double", ",", "float", ">>", ">>")]

        [InlineData("    List`3<<int, double, List<< <string, double, float>>   >>",
                    true,
                    StringSplitOptions.TrimEntries,
                    new[] { "<<", ">>", ",", " ", "<" },
                    "List`3", "<<", "int", ",", "double", ",", "List", "<<", "<", "string", ",", "double", ",", "float", ">>", ">>")]

        [InlineData("    List`3<<int, double, List<< <string, double, float>>   >>",
                    false,
                    StringSplitOptions.TrimEntries,
                    new[] { "<<", ">>", ",", " ", "<" },
                    "List`3", "int", "double", "List", "string", "double", "float")]

        [InlineData("<Split muti sentence. To ensure ? that all the sentence are split ... Correctly !>",
                    true,
                    StringSplitOptions.TrimEntries,
                    new[] { "...", "<", ">", ".", "!", "?" },
                    "<", "Split muti sentence", ".", "To ensure", "?", "that all the sentence are split", "...", "Correctly", "!", ">")]
        public void SplitByMultipleSeparators(string source, bool includeSeparator, StringSplitOptions options, string[] separators, params string[] expectedResults)
        {
            var args = source.OptiSplit(includeSeparator ? Models.StringIncludeSeparatorMode.Isolated : Models.StringIncludeSeparatorMode.None, StringComparison.Ordinal, options, separators);

            Check.That(args).IsNotNull()
                            .And
                            .CountIs(expectedResults.Length)
                            .And
                            .ContainsExactly(expectedResults);
        }

        [Theory]
        [InlineData("List", false, StringSplitOptions.RemoveEmptyEntries, new[] { "<<", ">>" }, "List")]
        [InlineData("List<<int>>", false, StringSplitOptions.RemoveEmptyEntries, new[] { "<<", ">>" }, "List", "int")]
        [InlineData("List<<int>>", true, StringSplitOptions.RemoveEmptyEntries, new[] { "<<", ">>" }, "List<<", "int>>")]
        [InlineData("<<List<<int>>", true, StringSplitOptions.RemoveEmptyEntries, new[] { "<<", ">>" }, "<<", "List<<", "int>>")]

        [InlineData("    List`3<<int, double, List<<string, double, float>>   >>",
                    true,
                    StringSplitOptions.RemoveEmptyEntries,
                    new[] { "<<", ">>", ",", " " },
                    "List`3<<", "int,", "double,", "List<<", "string,", "double,", "float>>", ">>")]

        [InlineData("    List`3<<int, double, List<< <string, double, float>>   >>",
                    true,
                    StringSplitOptions.TrimEntries,
                    new[] { "<<", ">>", ",", " ", "<" },
                    "List`3<<", "int,", "double,", "List<<", "<", "string,", "double,", "float>>", ">>")]

        [InlineData("    List`3<<int, double, List<< <string, double, float>>   >>",
                    false,
                    StringSplitOptions.TrimEntries,
                    new[] { "<<", ">>", ",", " ", "<" },
                    "List`3", "int", "double", "List", "string", "double", "float")]

        [InlineData("<Split muti sentence. To ensure ? that all the sentence are split ... Correctly !>",
                    true,
                    StringSplitOptions.TrimEntries,
                    new[] { "...", "<", ">", ".", "!", "?" },
                    "<", "Split muti sentence.", "To ensure ?", "that all the sentence are split ...", "Correctly !", ">")]

        public void SplitByMultipleSeparatorsAttached(string source, bool includeSeparator, StringSplitOptions options, string[] separators, params string[] expectedResults)
        {
            var args = source.OptiSplit(includeSeparator ? Models.StringIncludeSeparatorMode.AttachedToPrevious : Models.StringIncludeSeparatorMode.None, StringComparison.Ordinal, options, separators);

            Check.That(args).IsNotNull()
                            .And
                            .CountIs(expectedResults.Length)
                            .And
                            .ContainsExactly(expectedResults);
        }

        [Theory]
        [InlineData("List", false, StringSplitOptions.RemoveEmptyEntries, new[] { "<<", ">>" }, new string[0], "List")]
        [InlineData("List<<int>>", false, StringSplitOptions.RemoveEmptyEntries, new[] { "<<", ">>" }, new string[0], "List", "int")]
        [InlineData("List<<int>>", true, StringSplitOptions.RemoveEmptyEntries, new[] { "<<", ">>" }, new string[0], "List<<", "int>>")]
        [InlineData("<<List<<int>>", true, StringSplitOptions.RemoveEmptyEntries, new[] { "<<", ">>" }, new string[0], "<<", "List<<", "int>>")]

        [InlineData("    List`3<<int, double, List<<string, double, float>>   >>",
                    true,
                    StringSplitOptions.RemoveEmptyEntries,
                    new[] { "<<", ">>", ",", " " },
                    new string[0],
                    "List`3<<", "int,", "double,", "List<<", "string,", "double,", "float>>", ">>")]

        [InlineData("    List`3<<int, double, List<< <string, double, float>>   >>",
                    true,
                    StringSplitOptions.TrimEntries,
                    new[] { "<<", ">>", ",", " ", "<" },
                    new string[0],
                    "List`3<<", "int,", "double,", "List<<", "<", "string,", "double,", "float>>", ">>")]

        [InlineData("    List`3<<int, double, List<< <string, double, float>>   >>",
                    false,
                    StringSplitOptions.TrimEntries,
                    new[] { "<<", ">>", ",", " ", "<" },
                    new string[0],
                    "List`3", "int", "double", "List", "string", "double", "float")]

        [InlineData("<Split muti sentence. To ensure ? that all the sentence are split ... Correctly !>",
                    true,
                    StringSplitOptions.TrimEntries,
                    new[] { "...", "<", ">", ".", "!", "?" },
                    new string[0],
                    "<", "Split muti sentence.", "To ensure ?", "that all the sentence are split ...", "Correctly !", ">")]

        public void SplitByMultipleSeparatorsAttachedContext(string source, bool includeSeparator, StringSplitOptions options, string[] separators, string[] excluded, params string[] expectedResults)
        {
            var ctx = StringIndexedContext.Create(separators ?? EnumerableHelper<string>.ReadOnlyArray,
                                                  excluded ?? EnumerableHelper<string>.ReadOnlyArray);

            var args = source.OptiSplit(includeSeparator ? Models.StringIncludeSeparatorMode.AttachedToPrevious : Models.StringIncludeSeparatorMode.None,
                                        options,
                                        ctx);

            Check.That(args).IsNotNull()
                            .And
                            .CountIs(expectedResults.Length)
                            .And
                            .ContainsExactly(expectedResults);
        }

        [Theory]
        [InlineData("List<<<Remain<<After", false, StringSplitOptions.RemoveEmptyEntries, new[] { "<<" }, new [] { "<<<" }, "List<<<Remain", "After")]
        [InlineData("Lorem ipsum?Test Simple.Filter (string, Etc. next.)!A la con", false, StringSplitOptions.RemoveEmptyEntries, new[] { ".", "?", "!", ":" }, new [] { ".)", "Etc." }, "Lorem ipsum", "Test Simple", "Filter (string, Etc. next.)", "A la con")]
        public void SplitByMultipleSeparatorsAttachedContextWithExlude(string source, bool includeSeparator, StringSplitOptions options, string[] separators, string[] excluded, params string[] expectedResults)
        {
            var ctx = StringIndexedContext.Create(separators ?? EnumerableHelper<string>.ReadOnlyArray,
                                                  excluded ?? EnumerableHelper<string>.ReadOnlyArray);

            var args = source.OptiSplit(includeSeparator ? Models.StringIncludeSeparatorMode.AttachedToPrevious : Models.StringIncludeSeparatorMode.None,
                                        options,
                                        ctx);

            Check.That(args).IsNotNull()
                            .And
                            .CountIs(expectedResults.Length)
                            .And
                            .ContainsExactly(expectedResults);
        }

        [Theory]
        [InlineData("Test", false, StringSplitOptions.RemoveEmptyEntries, new[] { "TestALaCon" }, new string[0], "Test")]
        [InlineData("Test", false, StringSplitOptions.RemoveEmptyEntries, new string[0], new[] { "Test" }, "Test")]
        public void SplitByMultipleSeparatorsExt(string source, bool includeSeparator, StringSplitOptions options, string[] separators, string[] excluded, params string[] expectedResults)
        {
            var ctx = StringIndexedContext.Create(separators ?? EnumerableHelper<string>.ReadOnlyArray,
                                                  excluded ?? EnumerableHelper<string>.ReadOnlyArray);

            var args = source.OptiSplit(includeSeparator ? Models.StringIncludeSeparatorMode.AttachedToPrevious : Models.StringIncludeSeparatorMode.None,
                                        options,
                                        ctx);

            Check.That(args).IsNotNull()
                            .And
                            .CountIs(expectedResults.Length)
                            .And
                            .ContainsExactly(expectedResults);
        }

        [Theory]
        [InlineData(new [] { "a", "abc", "bc", "c", "cratos", "cr" }, new [] { "criter", "abcd" })]
        public void SplitContext(string[] separators, string[] excluded)
        {
            var ctx = StringIndexedContext.Create(separators, excluded);

            foreach (var s in separators)
            {
                var founded = ctx.Search(s, 0, out var include, out var deep);
                
                Check.That(founded).IsTrue();

                Check.That(include).IsNotNull();
                Check.That(include!.Value).IsTrue();

                Check.That(deep).IsNotNull();
                Check.That(deep!.Value).IsEqualTo(s.Length);
            }

            foreach (var s in excluded)
            {
                var founded = ctx.Search(s, 0, out var include, out var deep);

                Check.That(founded).IsTrue();

                Check.That(include).IsNotNull();
                Check.That(include!.Value).IsFalse();

                Check.That(deep).IsNotNull();
                Check.That(deep!.Value).IsEqualTo(s.Length);
            }
        }

        [Theory]
        [InlineData("ToLowerWithSeparator", "to_lower_with_separator", '_')]
        [InlineData("ToLowerWithSeparator", "to-lower-with-separator", '-')]
        [InlineData("     ToLowerWithSeparator", "     -to-lower-with-separator", '-')]
        public void ToLowerWithSeparator(string source, string target, char separator)
        {
            var result = source.ToLowerWithSeparator(separator);

            Check.That(result).IsNotNull().And.IsEqualTo(target);
        }

        [Theory]
        [InlineData("     ToLowerWithSeparator", "to-lower-with-separator", '-')]
        [InlineData("     ToLowerWithSeparator          ", "to-lower-with-separator", '-')]
        [InlineData("ToLowerWithSeparator     ", "to_lower_with_separator", '_')]
        [InlineData("     ToLowerWithSeparator     ", "to_lower_with_separator", '_')]
        public void ToLowerWithSeparator_With_Trim(string source, string target, char separator)
        {
            var result = source.AsSpan().Trim().ToLowerWithSeparator(separator);

            Check.That(result).IsNotNull().And.IsEqualTo(target);
        }
    }
}
