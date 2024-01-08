using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using CSharpFunctionalExtensions;
using ProjectMonitors.Monitor.Domain;
using ProjectMonitors.SeedWork.Domain;
//
// namespace ProjectMonitors.Monitor.App.Sites.Amazon
// {
//   [Monitor("amazon")]
//   public class AmazonFetcherFactory : ProductStatusFetcherFactoryBase
//   {
//     private static readonly Regex SkuRegex = new("([A-z0-9]{10,10})", RegexOptions.Compiled);
//     private readonly IMonitorHttpClientFactory _monitorHttpClientFactory;
//
//     public AmazonFetcherFactory(IServiceProvider serviceProvider, IMonitorHttpClientFactory monitorHttpClientFactory) :
//       base(serviceProvider)
//     {
//       _monitorHttpClientFactory = monitorHttpClientFactory;
//     }
//
//     public override Result<string> ParseRawTargetInput(string raw)
//     {
//       if (Uri.TryCreate(raw, UriKind.Absolute, out var uri))
//       {
//         var match = SkuRegex.Match(uri.Segments[3].TrimEnd('/'));
//         if (match.Success && uri.Host.Contains("amazon."))
//         {
//           return match.Groups[1].Value;
//         }
//       }
//       else
//       {
//         var match = SkuRegex.Match(raw);
//         if (match.Success)
//         {
//           return match.Groups[1].Value;
//         }
//       }
//
//       return Result.Failure<string>("Invalid format");
//     }
//
//     public override async ValueTask<WatchTarget> CreateTargetAsync(string raw, CancellationToken ct = default)
//     {
//       var result = ParseRawTargetInput(raw);
//       if (result.IsFailure)
//       {
//         throw new ArgumentException("Invalid raw sku value provided.", nameof(raw));
//       }
//
//       var sku = result.Value;
//       var pageUrl = new Uri($"https://www.amazon.com/dp/{sku}/");
//       var client = _monitorHttpClientFactory.CreateHttpClient();
//       var response = await client.GetAsync(pageUrl, ct);
//       if (!response.IsSuccessStatusCode)
//       {
//         throw new HttpRequestException("Non OK status code");
//       }
//
//       var responseString = await response.Content.ReadAsStringAsync(ct);
//       var ctx = BrowsingContext.New(Configuration.Default);
//       var doc = await ctx.OpenAsync(_ => _.Content(responseString), ct);
//       var photo = doc.GetElementById("landingImage").GetAttribute("src");
//       var title = doc.GetElementById("productTitle").Text();
//       var offerId = doc.QuerySelector("input#offerListingID").Attributes["value"].Value;
//       //todo: Add price when price #String
//       client.Dispose();
//       return new WatchTarget
//       {
//         Input = raw,
//         WatchersCount = 1,
//         ShopIconUrl =
//           "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a9/Amazon_logo.svg/1000px-Amazon_logo.svg.png",
//         ShopTitle = "amazon.com",
//         Products = new Dictionary<string, ProductSummary>
//         {
//           {
//             sku,
//             new ProductSummary
//             {
//               Sku = sku,
//               Picture = photo,
//               Title = title,
//               PageUrl = pageUrl,
//               Attributes =
//               {
//                 new ProductAttribute {Name = AmazonFetcher.OfferIdAttrName, Value = offerId}
//               }
//             }
//           }
//         }
//       };
//     }
//
//     public override IProductStatusFetcher CreateFetcher(WatchTarget target)
//     {
//       return new AmazonFetcher(_monitorHttpClientFactory.CreateHttpClient(), target);
//     }
//   }
// }


namespace ProjectMonitors.Monitor.App.Sites.Amazon
{
  [Monitor("amazon")]
  public class AmazonFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private static readonly Regex SkuRegex = new("([A-z0-9]{10,10})", RegexOptions.Compiled);
    private readonly IMonitorHttpClientFactory _monitorHttpClientFactory;

    public AmazonFetcherFactory(IServiceProvider serviceProvider, IMonitorHttpClientFactory monitorHttpClientFactory) :
      base(serviceProvider)
    {
      _monitorHttpClientFactory = monitorHttpClientFactory;
    }

    public override Result<string> ParseRawTargetInput(string raw)
    {
      if (Uri.TryCreate(raw, UriKind.Absolute, out var uri))
      {
        var match = SkuRegex.Match(uri.Segments[3].TrimEnd('/'));
        if (match.Success && uri.Host.Contains("amazon."))
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

      return Result.Failure<string>("Invalid format");
    }

    public override async ValueTask<WatchTarget> CreateTargetAsync(string raw, CancellationToken ct = default)
    {
      var result = ParseRawTargetInput(raw);
      if (result.IsFailure)
      {
        throw new ArgumentException("Invalid raw sku value provided.", nameof(raw));
      }

      var sku = result.Value;
      var pageUrl = new Uri($"https://www.amazon.com/dp/{sku}/");
      var client = _monitorHttpClientFactory.CreateHttpClient();
      var response = await client.GetAsync(pageUrl, ct);
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException("Non OK status code");
      }

      var responseString = await response.Content.ReadAsStringAsync(ct);
      var ctx = BrowsingContext.New(Configuration.Default);
      var doc = await ctx.OpenAsync(_ => _.Content(responseString), ct);
      var photo = doc.GetElementById("landingImage").GetAttribute("src");
      var title = doc.GetElementById("productTitle").Text();
      //todo: Add price when price #String
      client.Dispose();
      return new WatchTarget
      {
        Input = raw,
        WatchersCount = 1,
        ShopIconUrl =
          "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a9/Amazon_logo.svg/1000px-Amazon_logo.svg.png",
        ShopTitle = "amazon.com",
        Products = new Dictionary<string, ProductSummary>
        {
          {
            sku,
            new ProductSummary
            {
              Sku = sku,
              Picture = photo,
              Title = title,
              PageUrl = pageUrl
            }
          }
        }
      };
    }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target)
    {
      return new AmazonFetcher(_monitorHttpClientFactory.CreateHttpClient(), target.Input);
    }
  }
}