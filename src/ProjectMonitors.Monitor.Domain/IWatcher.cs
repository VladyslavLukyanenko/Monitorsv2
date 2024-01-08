using System;
using System.Threading;
using System.Threading.Tasks;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Monitor.Domain
{
  public interface IWatcher : IAsyncDisposable
  {
    ValueTask SpawnAsync(WatchTarget target, CancellationToken ct);
    event AsyncEventHandler<WatcherStatusChangedEventArgs> StatusChanged;
  }
}