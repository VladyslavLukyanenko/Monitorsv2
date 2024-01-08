using System.Threading;
using System.Threading.Tasks;

namespace ProjectMonitors.Monitor.Domain
{
  public interface IMonitorStatsCollector
  {
    void UpdateWatcherEntry(MonitorWatcherStats e, CancellationToken ct);
  }
}