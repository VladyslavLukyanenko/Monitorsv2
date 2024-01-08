using System;

namespace ProjectMonitors.Monitor.Domain
{
  public class MonitorWatcherStats
  {
    private MonitorWatcherStats()
    {
    }

    public static MonitorWatcherStats FromTarget(WatchTarget spec, IWatchStatus status)
    {
      var summary = spec.Products[status.TargetId];
      return new()
      {
        Title = summary.Title,
        ProductPic = summary.Picture,
        TargetId = status.TargetId,
        ProductUrl = summary.PageUrl?.ToString()
      };
    }

    public string TargetId { get; init; } = null!;
    public string? ProductUrl { get; init; } = null!;
    public string? ProductPic { get; init; } = null!;
    public string Title { get; init; } = null!;
    public DateTimeOffset Timestamp { get; private set; } = DateTimeOffset.UtcNow;
  }
}