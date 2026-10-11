// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.VGrains.Web
{
    using MARI.Framework.Core;
    using MARI.Framework.Core.Abstractions;
    using MARI.VGrains.Web.Abstractions;
    using MARI.VGrains.Web.Abstractions.Models;

    using Microsoft.Extensions.Logging;

    using Orleans.Concurrency;
    using Orleans.Placement;

    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// VGrain in charge to fetch html page based on URL
    /// </summary>
    /// <seealso cref="VGrainBase" />
    /// <seealso cref="IHtmlCollectorVGrain" />
    [StatelessWorker]
    [RandomPlacement]
    public sealed class HtmlCollectorVGrain : VGrainBase<IHtmlCollectorVGrain>, IHtmlCollectorVGrain
    {
        #region Fields

        /// <summary>
        /// Maximum page size accepted (10 MB)
        /// </summary>
        private const long MAX_CONTENT_SIZE = 10 * 1024 * 1024;

        private readonly IHttpClientFactory _httpClientFactory;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlCollectorVGrain"/> class.
        /// </summary>
        public HtmlCollectorVGrain(ILogger<IHtmlCollectorVGrain> logger,
                                  IHttpClientFactory httpClientFactory)
            : base(logger)
        {
            this._httpClientFactory = httpClientFactory;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public async Task<HtmlPage> FetchPageAsync(Uri pageUrl, IExecutionContext executionContext)
        {
            ArgumentNullException.ThrowIfNull(pageUrl);

            if (!pageUrl.IsAbsoluteUri || (pageUrl.Scheme != Uri.UriSchemeHttp && pageUrl.Scheme != Uri.UriSchemeHttps))
                throw new ArgumentException("Only absolute http/https URLs are supported : " + pageUrl, nameof(pageUrl));

            var token = executionContext?.CancellationToken ?? default;

            using (var client = this._httpClientFactory.CreateClient())
            {
                // Content is buffered (default completion option) and HttpClient throws if it exceeds this limit
                client.MaxResponseContentBufferSize = MAX_CONTENT_SIZE;

                using (var pageRawResponse = await client.GetAsync(pageUrl, token))
                {
                    pageRawResponse.EnsureSuccessStatusCode();

                    var contentString = pageRawResponse.Content is null
                                                ? string.Empty
                                                : await pageRawResponse.Content.ReadAsStringAsync(token);

                    return new HtmlPage(pageUrl, contentString);
                }
            }
        }

        #endregion
    }
}
