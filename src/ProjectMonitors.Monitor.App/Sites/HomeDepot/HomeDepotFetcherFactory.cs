using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using ProjectMonitors.Monitor.Domain;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Monitor.App.Sites.HomeDepot
{
  [Monitor("homedepot")]
  public class HomeDepotFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IMonitorHttpClientFactory _monitorHttpClientFactory;

    public HomeDepotFetcherFactory(IServiceProvider serviceProvider, IJsonSerializer jsonSerializer,
      IMonitorHttpClientFactory monitorHttpClientFactory)
      : base(serviceProvider)
    {
      _jsonSerializer = jsonSerializer;
      _monitorHttpClientFactory = monitorHttpClientFactory;
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
      return new HomeDepotFetcher(_monitorHttpClientFactory.CreateHttpClient(), target.Input, _jsonSerializer);
    }
  }
}