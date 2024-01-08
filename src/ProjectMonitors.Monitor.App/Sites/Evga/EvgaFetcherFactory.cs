﻿using System;
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

namespace ProjectMonitors.Monitor.App.Sites.Evga
{
  [Monitor("evga")]
  public class EvgaFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private static readonly Regex SkuRegex =
      new("([0-9]{3,3}-[A-Z]{2,2}-[A-Z0-9]{4,4}-[A-Z]{2,2})", RegexOptions.Compiled);

    private readonly IMonitorHttpClientFactory _monitorHttpClientFactory;

    public EvgaFetcherFactory(IServiceProvider serviceProvider, IMonitorHttpClientFactory monitorHttpClientFactory)
      : base(serviceProvider)
    {
      _monitorHttpClientFactory = monitorHttpClientFactory;
    }

    public override Result<string> ParseRawTargetInput(string raw)
    {
      if (Uri.TryCreate(raw, UriKind.Absolute, out var uri))
      {
        var match = SkuRegex.Match(uri.Query);
        if (match.Success && uri.Host.Contains("evga"))
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
      var client = _monitorHttpClientFactory.CreateHttpClient();
      var pageUrl = new Uri($"https://www.evga.com/products/product.aspx?pn={sku}");
      var response = await client.GetAsync(pageUrl, ct);
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException("Non OK status code");
      }

      var responseString = await response.Content.ReadAsStringAsync(ct);
      var ctx = BrowsingContext.New(Configuration.Default);
      var doc = await ctx.OpenAsync(_ => _.Content(responseString), ct);
      var photo = doc.GetElementById("LFrame_imgPrdLarge").GetAttribute("src");
      var title = doc.GetElementById("LFrame_lblProductName").Text();
      //todo: Add price when price #String
      client.Dispose();
      return new WatchTarget
      {
        Input = sku,
        WatchersCount = 1,
        ShopIconUrl = "https://zonaactual.es/wp-content/uploads/2021/01/evga-logo-copertina-91470.768x432.jpg",
        ShopTitle = "evga.com",
        Products = new Dictionary<string, ProductSummary>
        {
          {
            sku,
            new ProductSummary
            {
              Sku = sku, Picture = photo, Title = title,
              PageUrl = pageUrl
            }
          }
        }
      };
    }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target)
    {
      return new EvgaFetcher(_monitorHttpClientFactory.CreateHttpClient(), new Uri(target.Input));
    }
  }
}