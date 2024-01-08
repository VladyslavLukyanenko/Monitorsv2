using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using ProjectMonitors.Monitor.Domain;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Monitor.App.Sites.FakeShop
{
  [Monitor("fake_shop")]
  public class FakeShopFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private readonly IMonitorHttpClientFactory _monitorHttpClientFactory;
    private readonly IJsonSerializer _jsonSerializer;

    public FakeShopFetcherFactory(IMonitorHttpClientFactory monitorHttpClientFactory, IServiceProvider serviceProvider,
      IJsonSerializer jsonSerializer)
      : base(serviceProvider)
    {
      _monitorHttpClientFactory = monitorHttpClientFactory;
      _jsonSerializer = jsonSerializer;
    }

    public override Result<string> ParseRawTargetInput(string raw)
    {
      throw new NotImplementedException();
    }

    public override ValueTask<WatchTarget> CreateTargetAsync(string raw, CancellationToken ct = default)
    {
      var result = ParseRawTargetInput(raw);
      if (result.IsFailure)
      {
        throw new ArgumentException("Invalid raw sku value provided.", nameof(raw));
      }
      throw new NotImplementedException();
    }

    public override IProductStatusFetcher CreateFetcher(WatchTarget target)
    {
      return new FakeShopFetcher(new Uri(target.Input), target, _monitorHttpClientFactory.CreateHttpClient(), _jsonSerializer);
    }
  }
}