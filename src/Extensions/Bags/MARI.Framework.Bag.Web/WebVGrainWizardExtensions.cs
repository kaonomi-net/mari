// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Abstractions.Configurations
{
    using MARI.Framework.Configurations;
    using MARI.VGrains.Web;
    using MARI.VGrains.Web.Abstractions;

    using Microsoft.Extensions.DependencyInjection;

    public static class WebVGrainWizardExtensions
    {
        /// <summary>
        /// Add web vgrains
        /// </summary>
        public static IMARINodeVGrainWizard UseWebVGrains(this IMARINodeVGrainWizard inst)
        {
            inst.Root.ConfigureServices(services => services.AddHttpClient());
            inst.Add<IHtmlCollectorVGrain, HtmlCollectorVGrain>();
            return inst;
        }
    }
}
