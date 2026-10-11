// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Core.UnitTests.Services
{
    using MARI.Toolbox.Core.UnitTests.Resources;
    using MARI.Toolbox.Services;

    using NFluent;

    using System;
    using System.Globalization;

    /// <summary>
    /// Test for <see cref="GlobalizationStringResourceProvider"/>
    /// </summary>
    public sealed class GlobalizationStringResourceProviderUTest
    {
        //[Fact]
        //public void Use_Providers()
        //{
        //    throw new Exception();
        //}

        //[Fact]
        //public void Use_Cache()
        //{
        //    throw new Exception();
        //}

        [Fact]
        public void Use_Multi_Culture()
        {
            var resxProvider = new GlobalizationStringResourceFromRESXSourceProvider<SR>();

            var provider = new GlobalizationStringResourceProvider(new [] { resxProvider });

            var englishCulture = CultureInfo.GetCultureInfo("EN");
            var frenchCulture = CultureInfo.GetCultureInfo("FR");
            var spainCulture = CultureInfo.GetCultureInfo("ES");

            CultureInfo.CurrentCulture = englishCulture;
            CultureInfo.CurrentUICulture = englishCulture;

            var defaultValue = provider.GetResource("Sample");

            var enValue = provider.GetResource("Sample", englishCulture);
            var frValue = provider.GetResource("Sample", frenchCulture);
            var esValue = provider.GetResource("Sample", spainCulture);

            Check.That(defaultValue).IsEqualTo("EN");
            Check.That(enValue).IsEqualTo("EN");
            Check.That(frValue).IsEqualTo("FR");
            Check.That(esValue).IsEqualTo("ES");
        }
    }
}
