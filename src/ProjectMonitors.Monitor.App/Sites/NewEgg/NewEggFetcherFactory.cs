using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using CSharpFunctionalExtensions;
using ProjectMonitors.Monitor.Domain;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Monitor.App.Sites.NewEgg
{
  [Monitor("newegg")]
  public class NewEggFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private static readonly Regex SkuRegex = new("([0-9A-Z]{15,15})", RegexOptions.Compiled);
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IMonitorHttpClientFactory _monitorHttpClientFactory;

    public NewEggFetcherFactory(IServiceProvider serviceProvider, IJsonSerializer jsonSerializer,
      IMonitorHttpClientFactory monitorHttpClientFactory)
      : base(serviceProvider)
    {
      _jsonSerializer = jsonSerializer;
      _monitorHttpClientFactory = monitorHttpClientFactory;
    }

    public override Result<string> ParseRawTargetInput(string raw)
    {
      if (Uri.TryCreate(raw, UriKind.Absolute, out var uri))
      {
        var match = SkuRegex.Match(uri.Segments[3]);
        if (match.Success && uri.Host.Contains("newegg.com"))
        {
          return match.Groups[1].Value;
        }
      }
      else
      {
        var match = SkuRegex.Match(raw);
        if (match.Success)
        {
          return match.Groups[1].Value;
        }
      }

      return Result.Failure<string>("Invalid format provided");
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
      var pageUrl = new Uri($"https://www.newegg.com/p/{sku}");
      var response = await client.GetAsync(pageUrl, ct);
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException("Non OK status code");
      }

      var responseString = await response.Content.ReadAsStringAsync(ct);
      var ctx = BrowsingContext.New(Configuration.Default);
      var doc = await ctx.OpenAsync(_ => _.Content(responseString), ct);
      var photo = doc.GetElementsByClassName("product-view-img-original")[0].GetAttribute("src");
      var title = doc.GetElementsByClassName("product-title")[0].Children.Last().Text();
      //todo: Add price when price #String
      client.Dispose();
      return new WatchTarget
      {
        Input = sku,
        WatchersCount = 1,
        ShopIconUrl = "https://c1.neweggimages.com/WebResource/Themes/logo_newegg_400400.png",
        ShopTitle = "newegg.com",
        Products = new Dictionary<string, ProductSummary>
        {
          {
            sku,
            new ProductSummary {Sku = sku, Picture = photo, Title = title, PageUrl = pageUrl}
          }
        }
      };
    }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target)
    {
      return new NewEggFetcher(_monitorHttpClientFactory.CreateHttpClient(), target.Input, _jsonSerializer);
    }
  }
}