using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectMonitors.Monitor.Domain
{
  public interface IWatchersManager : IAsyncDisposable
  {
    ValueTask SpawnWatcherAsync(WatchTarget watchTarget, ProductStatus initialStatus, CancellationToken ct);
    ValueTask StopAllAsync(CancellationToken ct);
    void UseSettings(MonitorSettings settings);
  }
}