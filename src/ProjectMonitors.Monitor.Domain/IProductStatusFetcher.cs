using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace ProjectMonitors.Monitor.Domain
{
  public interface IProductStatusFetcher
  {
    ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct);
  }
}