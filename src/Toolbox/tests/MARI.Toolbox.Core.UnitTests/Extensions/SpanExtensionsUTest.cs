// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Core.UnitTests.Extensions
{
    using NFluent;

    using System;

    /// <summary>
    /// Test for <see cref="SpanExtensions"/>
    /// </summary>
    public sealed class SpanExtensionsUTest
    {
        [Theory]

        // Simple
        [InlineData("azertyuiopqsdfghjklmwxcvbn", '_', (char[])['a', 't', 'p'], "_zer_yuio_qsdfghjklmwxcvbn")]

        // Multiple time same
        [InlineData("azertyuiopqsdfghjklmwxcvbnazertyuiopqsdfghjklmwxcvbn", '_', (char[])['a', 't', 'p'], "_zer_yuio_qsdfghjklmwxcvbn_zer_yuio_qsdfghjklmwxcvbn")]

        // Contigue & last 
        [InlineData("azertyuiopqsdfghjklmwxcvbnazertyuiopqsdfghjklmwxcvbn", '_', (char[])['a', 't', 'p', 'n'], "_zer_yuio_qsdfghjklmwxcvb__zer_yuio_qsdfghjklmwxcvb_")]

        // No replacement
        [InlineData("azertyuiopqsdfghjklmwxcvbnazertyuiopqsdfghjklmwxcvbn", '_', (char[])[], "azertyuiopqsdfghjklmwxcvbnazertyuiopqsdfghjklmwxcvbn")]
        public void Span_ReplaceMany(string source, char replacement, char[] search, string expected)
        {
            ReadOnlySpan<char> spanSource = source;

            var result = spanSource.ReplaceMany(replacement, search);

            Check.That(result.ToString()).IsNotEmpty().And.Equals(expected);
        }

        [Theory]

        // No Replacement
        [InlineData("azertyuiopqsdfghjklmwxcvbn", "azertyuiopqsdfghjklmwxcvbn")]

        // ALL Replacement
        [InlineData("AZERTYUIOPQSDFGHJKLMWXCVBN", "azertyuiopqsdfghjklmwxcvbn")]

        // ALL Replacement with other char
        [InlineData("AZERTYU+I_O-P+Q*$S(DFGH=JK)LMWXCVBN", "azertyu+i_o-p+q*$s(dfgh=jk)lmwxcvbn")]

        public void Span_ToLower(string source, string expected)
        {
            ReadOnlySpan<char> spanSource = source;

            var result = SpanExtensions.ToLower(spanSource);

            Check.That(result.ToString()).IsNotEmpty().And.Equals(expected);
        }

        [Theory]

        // No Replacement
        [InlineData("AZERTYUIOPQSDFGHJKLMWXCVBN", "AZERTYUIOPQSDFGHJKLMWXCVBN")]

        // ALL Replacement
        [InlineData("azertyuiopqsdfghjklmwxcvbn", "AZERTYUIOPQSDFGHJKLMWXCVBN")]

        // ALL Replacement with other char
        [InlineData("azertyu+i_o-p+q*$s(dfgh=jk)lmwxcvbn", "AZERTYU+I_O-P+Q*$S(DFGH=JK)LMWXCVBN")]

        public void Span_ToUpper(string source, string expected)
        {
            ReadOnlySpan<char> spanSource = source;

            var result = SpanExtensions.ToUpper(spanSource);

            Check.That(result.ToString()).IsNotEmpty().And.Equals(expected);
        }
    }
}
