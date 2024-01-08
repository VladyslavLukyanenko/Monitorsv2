using System.Collections.Generic;

namespace ProjectMonitors.Monitor.Domain
{
  public interface IWatcherFactory
  {
    IWatcher Create(WatchTarget target, IDictionary<string, ProductStatus> initialStatuses);
  }
}