using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using ProjectMonitors.SeedWork.Domain;
using ProjectMonitors.Crawler.Domain;

namespace ProjectMonitors.Crawler.Sites.Fanatics
{
  public class FanaticsScraper : IScraper
  {
    private const int ItemsPerPage = 72;
    private readonly IScraperHttpClientFactory _httpClientFactory;
    private readonly ScraperConfig _config;
    private readonly IJsonSerializer _jsonSerializer;

    public FanaticsScraper(IScraperHttpClientFactory httpClientFactory, ScraperConfig config, IJsonSerializer jsonSerializer)
    {
      _httpClientFactory = httpClientFactory;
      _config = config;
      _jsonSerializer = jsonSerializer;
    }

    public ScraperConfig Config => _config;

    public ValueTask<int> GetProductsCountAsync(CancellationToken ct = default)
    {
      throw new NotImplementedException();
    }

    public async IAsyncEnumerable<Product> ParseProductsAsync(Uri pageUrl, int pageIdx,
      [EnumeratorCancellation] CancellationToken ct)
    {
      using var client = _httpClientFactory.CreateClient();
      var request = new HttpRequestMessage(HttpMethod.Get, pageUrl);
      var response = await client.SendAsync(request, ct);
      var content = await response.Content.ReadAsStreamAsync(ct);
      var bctx = BrowsingContext.New();
      var doc = await bctx.OpenAsync(_ => _.Content(content), ct);
      var productElements = doc.QuerySelectorAll(".product-grid-container .column");
      for (var itemIdx = 0; itemIdx < productElements.Length; itemIdx++)
      {
        var productElement = productElements[itemIdx];
        yield return ParseProduct(productElement, pageUrl, pageIdx, itemIdx);
      }
    }

    private Product ParseProduct(IElement root, Uri pageUrl, int pageIdx, int elementIdx)
    {
      var container = root.QuerySelector(".product-image-container > a");
      var productUrl = pageUrl.Authority.TrimEnd('/') + container.Attributes["href"]?.Value;
      var productPic = container.QuerySelector("img").Attributes["src"].Value;
      if (productPic.StartsWith("//"))
      {
        productPic = "https:" + productPic;
      }

      var titleEl = root.QuerySelector(".product-card-title > a");
      var title = titleEl.TextContent.Trim();
      var id = titleEl.Attributes["data-trk-id"].Value;
      var rawPrice = root.QuerySelector(".money-value .sr-only").TextContent.Trim().Substring(1);
      var price = decimal.Parse(rawPrice);

      return new Product
      {
        Title = title,
        Id = id,
        PageIdx = pageIdx,
        GlobalIndex = elementIdx,
        ImageUrl = productPic,
        ProductUrl = productUrl,
        Price = price
      };
    }

    public async ValueTask<IList<Uri>> ExtractPageUrlsAsync(CancellationToken ct = default)
    {
      using var client = _httpClientFactory.CreateClient();
      var query = Uri.EscapeDataString(_config.Query);
      var pageUrl = $"https://www.fanatics.com/?query={query}&_ref=p-HP:m-SEARCH";
      var initialPageRequest = new HttpRequestMessage(HttpMethod.Get, pageUrl);
      var initialPage = await client.SendAsync(initialPageRequest, ct);
      var initialPageContent = await initialPage.Content.ReadAsStreamAsync(ct);
      var ctx = BrowsingContext.New();
      var doc = await ctx.OpenAsync(_ => _.Content(initialPageContent), ct);
      var scripts = doc.QuerySelectorAll("script");
      var platformDataVarDecl = "var __platform_data__=";
      var stateScript = scripts.FirstOrDefault(_ => _.TextContent.StartsWith(platformDataVarDecl));
      if (stateScript == null)
      {
        throw new InvalidOperationException("Can't find prerendered state");
      }

      var payloadStr = stateScript.TextContent.Substring(platformDataVarDecl.Length);
      var appState = await _jsonSerializer.DeserializeAsync<FanaticsAppStateData>(payloadStr, ct);
      var pagesCount = (int) Math.Ceiling(appState!.TotalProductCount / (double) ItemsPerPage);
      return Enumerable.Range(1, pagesCount + 1)
        .Select(no => CreatePageUrl(no - 1, ItemsPerPage))
        .ToArray();
    }

    public Uri CreatePageUrl(int idx, int itemsPerPage)
    {
      return new(
        $"https://www.fanatics.com/?pageSize={itemsPerPage}&pageNumber={idx + 1}&sortOption=TopSellers&query={_config.Query}");
    }
  }
}