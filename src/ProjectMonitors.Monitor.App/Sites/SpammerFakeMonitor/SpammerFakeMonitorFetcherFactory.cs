using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using ProjectMonitors.Monitor.Domain;

namespace ProjectMonitors.Monitor.App.Sites.SpammerFakeMonitor
{
  [Monitor("spammer_fake_monitor")]
  public class SpammerFakeMonitorFetcherFactory : ProductStatusFetcherFactoryBase
  {
    public SpammerFakeMonitorFetcherFactory(IServiceProvider serviceProvider)
      : base(serviceProvider)
    {
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

    public override IProductStatusFetcher CreateFetcher(WatchTarget target) => new SpammerFakeMonitorFetcher();
  }
}