using System.Collections.Generic;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Monitor.Domain
{
  public class WatchTarget
  {
    public string Input { get; init; } = null!;
    public int WatchersCount { get; init; } = 1;

    public string? ShopIconUrl { get; init; }
    public string? ShopTitle { get; init; }
    public IDictionary<string, ProductSummary> Products { get; init; } = new Dictionary<string, ProductSummary>();
  }
}