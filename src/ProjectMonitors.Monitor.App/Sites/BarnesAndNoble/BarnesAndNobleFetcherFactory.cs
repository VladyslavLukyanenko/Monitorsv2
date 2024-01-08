using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using ProjectMonitors.Monitor.Domain;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Monitor.App.Sites.BarnesAndNoble
{
  [Monitor("barnesandnoble")]
  public class BarnesAndNobleFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IMonitorHttpClientFactory _monitorHttpClientFactory;

    public BarnesAndNobleFetcherFactory(IJsonSerializer jsonSerializer,
      IMonitorHttpClientFactory monitorHttpClientFactory, IServiceProvider serviceProvider)
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
      return new BarnesAndNobleFetcher(_monitorHttpClientFactory.CreateHttpClient(), new Uri(target.Input),
        _jsonSerializer);
    }
  }
}