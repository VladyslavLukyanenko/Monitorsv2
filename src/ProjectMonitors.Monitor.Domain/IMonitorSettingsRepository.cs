using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectMonitors.Monitor.Domain
{
  public interface IMonitorSettingsRepository
  {
    ValueTask<MonitorSettings?> GetSettingsAsync(MonitorInfo monitorInfo, CancellationToken ct);

    ValueTask<bool> UpdateTargetsIfNotChangedAsync(MonitorInfo monitorInfo, DateTimeOffset targetChanges,
      IEnumerable<WatchTarget> targets, CancellationToken ct = default);
  }
}