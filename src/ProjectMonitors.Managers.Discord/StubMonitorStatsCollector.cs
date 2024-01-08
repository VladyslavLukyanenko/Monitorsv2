using System.Threading;
using ProjectMonitors.Monitor.Domain;

namespace ProjectMonitors.Managers.Discord
{
  public class StubMonitorStatsCollector : IMonitorStatsCollector
  {
    public void UpdateWatcherEntry(MonitorWatcherStats e, CancellationToken ct)
    {
      // noop
    }
  }
}