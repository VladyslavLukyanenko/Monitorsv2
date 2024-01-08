using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using ProjectMonitors.Monitor.Domain;

namespace ProjectMonitors.Monitor.App.Sites.Xbox
{
  [Monitor("xbox")]
  public class XboxFetcherFactory : ProductStatusFetcherFactoryBase
  {
    private readonly IMonitorHttpClientFactory _monitorHttpClientFactory;

    public XboxFetcherFactory(IServiceProvider serviceProvider, IMonitorHttpClientFactory monitorHttpClientFactory)
      : base(serviceProvider)
    {
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
      return new XboxFetcher(_monitorHttpClientFactory.CreateHttpClient(), new Uri(target.Input));
    }
  }
}