using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectMonitors.Crawler.Domain
{
  public interface IScraper
  {
    ScraperConfig Config { get; }
    ValueTask<int> GetProductsCountAsync(CancellationToken ct = default);
    IAsyncEnumerable<Product> ParseProductsAsync(Uri pageUrl, int pageIdx, CancellationToken ct);
    ValueTask<IList<Uri>> ExtractPageUrlsAsync(CancellationToken ct = default);
    Uri CreatePageUrl(int idx, int itemsPerPage);
    // Task<ProductPage> FetchProductPageAsync(Uri pageUrl, int pageIdx, int requestsItemsPerPage, CancellationToken ct);
  }
}