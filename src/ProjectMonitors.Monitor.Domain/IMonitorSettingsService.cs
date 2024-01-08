using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectMonitors.Monitor.Domain
{
  public interface IMonitorSettingsService
  {
    IObservable<MonitorSettings> Settings { get; }

    ValueTask<MonitorSettings?> UpdateTargetsIfNotChangedAsync(MonitorInfo monitorInfo, DateTimeOffset targetChanges,
      IEnumerable<WatchTarget> targets, CancellationToken ct = default);
  }
}