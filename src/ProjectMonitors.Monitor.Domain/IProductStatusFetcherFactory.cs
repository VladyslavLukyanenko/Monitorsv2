using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace ProjectMonitors.Monitor.Domain
{
  public interface IProductStatusFetcherFactory
  {
    string TargetMonitorSlug { get; }

    Result<string> ParseRawTargetInput(string raw);
    ValueTask<WatchTarget> CreateTargetAsync(string raw, CancellationToken ct = default);
    IProductStatusFetcher CreateFetcher(WatchTarget target);
  }
}