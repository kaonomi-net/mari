// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.VGrains.Web.Abstractions
{
    using MARI.Framework.Core.Abstractions;
    using MARI.VGrains.Web.Abstractions.Models;

    /// <summary>
    /// VGrain is charge to extract html from web page address
    /// </summary>
    /// <seealso cref="IVGrain" />
    public interface IHtmlCollectorVGrain : IVGrain
    {
        /// <summary>
        /// Fetches an html page through it's web URL
        /// </summary>
        Task<HtmlPage> FetchPageAsync(Uri pageUrl, IExecutionContext executionContext);
    }
}