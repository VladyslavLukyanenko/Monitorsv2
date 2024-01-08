using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectMonitors.Monitor.Domain
{
  public class MonitorSettings
  {
    public static readonly TimeSpan DefaultDelayOnAvailable = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan DefaultDelayOnUnavailable = TimeSpan.FromMilliseconds(500);

    public string MonitorSlug { get; init; } = null!;
    public string MonitorName { get; init; } = null!;

    public DateTimeOffset UpdatedAt { get; init; }
    public IList<Uri> Proxies { get; init; } = new List<Uri>();
    public IList<string> UserAgents { get; init; } = new List<string>();
    public IList<WatchTarget> Targets { get; init; } = new List<WatchTarget>();
    public ProductStatus InitialStatus { get; init; }

    public TimeSpan? DelayOnAvailable { get; init; }
    public TimeSpan? DelayOnUnavailable { get; init; }

    public AntibotProtectionConfig AntibotConfig { get; init; } = new();

    public int CalculateTotalWorkersCount()
    {
      return Targets.Select(t => t.WatchersCount).Sum();
    }
  }
}