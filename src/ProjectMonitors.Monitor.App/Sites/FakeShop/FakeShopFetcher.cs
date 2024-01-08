using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using ProjectMonitors.Monitor.Domain;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Monitor.App.Sites.FakeShop
{
  [Monitor("fake_shop")]
  public class FakeShopFetcher : IProductStatusFetcher
  {
    private readonly Uri _productUrl;
    private readonly WatchTarget _target;
    private readonly HttpClient _httpClient;
    private readonly IJsonSerializer _jsonSerializer;

    public FakeShopFetcher(Uri productUrl, WatchTarget target, HttpClient httpClient, IJsonSerializer jsonSerializer)
    {
      _productUrl = productUrl;
      _target = target;
      _httpClient = httpClient;
      _jsonSerializer = jsonSerializer;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var request = new HttpRequestMessage(HttpMethod.Get, _productUrl);
      return await StatusFetchResult.ProcessResultAsync(request, _httpClient, ct, async result =>
      {
        var data = await _jsonSerializer.DeserializeAsync<FakeShopResponse>(result.RawResponse, ct);
        var url = _productUrl.ToString();
        var product = _target.Products[_target.Input];
        var timestampAttr = new ProductAttribute {Name = "Timestamp", Value = DateTimeOffset.UtcNow.ToString("O")};
        product.UpdateAttr(timestampAttr);
        result.AddStatus(url, data?.IsAvailable ?? false);

        return result;
      });
    }
  }
}