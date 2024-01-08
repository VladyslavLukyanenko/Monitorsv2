using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using ProjectMonitors.Crawler.Domain;

namespace ProjectMonitors.Crawler.Sites.FakeShop
{
  public class FakeShopScraper : IScraper
  {
    private const bool WithDelay = true;
    private readonly IScraperHttpClientFactory _httpClientFactory;

    public FakeShopScraper(IScraperHttpClientFactory httpClientFactory)
    {
      _httpClientFactory = httpClientFactory;
    }

    public ScraperConfig Config { get; } = new() {ItemsPerPage = 6};

    public async ValueTask<int> GetProductsCountAsync(CancellationToken ct = default)
    {
      using var client = _httpClientFactory.CreateClient();
      var request = new HttpRequestMessage(HttpMethod.Get, CreatePageUrl(0, Config.ItemsPerPage));
      var response = await client.SendAsync(request, ct);
      var content = await response.Content.ReadAsStreamAsync(ct);
      var bctx = BrowsingContext.New();
      var doc = await bctx.OpenAsync(_ => _.Content(content), ct);
      var countHolder = doc.QuerySelector(".products-count").TextContent;

      return int.Parse(countHolder);
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
      var productElements = doc.QuerySelectorAll(".product-card");

      var baseIdx = Config.ItemsPerPage * pageIdx;
      for (var itemIdx = 0; itemIdx < productElements.Length; itemIdx++)
      {
        var productElement = productElements[itemIdx];
        yield return ParseProduct(productElement, pageIdx, itemIdx + baseIdx);
      }
    }

    private Product ParseProduct(IElement root, int pageIdx, int itemIdx)
    {
      var titleEl = root.QuerySelector(".product-title");
      var title = titleEl.TextContent.Trim();
      var productUrl = root.QuerySelector(".product-link").Attributes["href"].Value;
      var productPic = root.QuerySelector(".product-pic").Attributes["src"].Value;
      var rawPrice = root.QuerySelector(".product-price").TextContent.Trim().Substring(1);
      var price = decimal.Parse(rawPrice);

      return new Product
      {
        Title = title,
        Id = productUrl,
        PageIdx = pageIdx,
        GlobalIndex = itemIdx,
        ImageUrl = productPic,
        ProductUrl = productUrl,
        Price = price
      };
    }

    public async ValueTask<IList<Uri>> ExtractPageUrlsAsync(CancellationToken ct = default)
    {
      using var client = _httpClientFactory.CreateClient();
      var pageUrl = CreatePageUrl(0, Config.ItemsPerPage);
      var request = new HttpRequestMessage(HttpMethod.Get, pageUrl);
      var response = await client.SendAsync(request, ct);
      var content = await response.Content.ReadAsStreamAsync(ct);
      var bctx = BrowsingContext.New();
      var doc = await bctx.OpenAsync(_ => _.Content(content), ct);
      var pageItems = doc.QuerySelectorAll(".page-item > a");


      var baseUrl = new Uri($"http://localhost:5000?withdelay={WithDelay}");
      return pageItems.Skip(1)
        .Take(pageItems.Length - 2)
        .Select(e => e.Attributes["href"].Value)
        .Select(rawUrl => new Uri(baseUrl, rawUrl))
        .ToList();
    }

    public Uri CreatePageUrl(int idx, int itemsPerPage)
    {
      return new($"http://localhost:5000/?withDelay={WithDelay}&pageIdx={idx}");
    }
  }
}