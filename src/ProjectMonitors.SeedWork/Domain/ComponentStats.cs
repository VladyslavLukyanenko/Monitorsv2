using System;

namespace ProjectMonitors.SeedWork.Domain
{
  public class ComponentStats
  {
    public string ComponentType { get; init; } = null!;
    public string ComponentName { get; init; } = null!;
    public string Stats { get; init; } = null!;
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
  }
}