using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using CSharpFunctionalExtensions;
using ProjectMonitors.Monitor.Domain;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Monitor.App.Sites.Panini
{
  [Monitor("panini")]
  public class PaniniFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IMonitorHttpClientFactory _monitorHttpClientFactory;

    public PaniniFetcherFactory(IServiceProvider serviceProvider, IJsonSerializer jsonSerializer,
      IMonitorHttpClientFactory monitorHttpClientFactory)
      : base(serviceProvider)
    {
      _jsonSerializer = jsonSerializer;
      _monitorHttpClientFactory = monitorHttpClientFactory;
    }

    public override Result<string> ParseRawTargetInput(string raw)
    {
      if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri) || !uri.Host.Contains("paniniamerica.net"))
      {
        return Result.Failure<string>("Invalid URL provided");
      }

      return uri.Segments[1].Split('.')[0];
    }

    public override async ValueTask<WatchTarget> CreateTargetAsync(string raw, CancellationToken ct = default)
    {
      var result = ParseRawTargetInput(raw);
      if (result.IsFailure)
      {
        throw new ArgumentException("Invalid raw sku value provided.", nameof(raw));
      }

      var sku = result.Value;
      var client = _monitorHttpClientFactory.CreateHttpClient();
      var pageUrl = new Uri($"https://www.paniniamerica.net/{sku}.html");
      var response = await client.GetAsync(pageUrl, ct);
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException("Non OK status code");
      }

      var responseString = await response.Content.ReadAsStringAsync(ct);
      var ctx = BrowsingContext.New(Configuration.Default);
      var doc = await ctx.OpenAsync(_ => _.Content(responseString), ct);
      var photo = doc.QuerySelector("div.row.d-flex > div.ce-p-images > div.p-image > div.top_level").Children[0]
        .GetAttribute("src");
      var title = doc.QuerySelector("div.row.d-flex > div.ce-p-details > div").Children[0].Text();
      //todo: Add price when price #String
      client.Dispose();
      return new WatchTarget
      {
        Input = sku,
        WatchersCount = 1,
        ShopIconUrl = "http://photos.prnewswire.com/prnfull/20141104/156449LOGO",
        ShopTitle = "paniniamerica.net",
        Products = new Dictionary<string, ProductSummary>
        {
          {sku, new ProductSummary {Sku = sku, Picture = photo, Title = title, PageUrl = pageUrl}}
        }
      };
    }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target)
    {
      return new PaniniFetcher(_monitorHttpClientFactory.CreateHttpClient(), target.Input, _jsonSerializer);
    }
  }
}