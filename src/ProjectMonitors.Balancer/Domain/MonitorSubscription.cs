using System.Collections.Generic;

namespace ProjectMonitors.Balancer.Domain
{
  public class MonitorSubscription
  {
    public int Id { get; init; }
    public string Slug { get; init; } = null!;
    public string DiscordWebhookUrl { get; init; } = null!;
    public IList<string> AllowedTargets { get; init; } = new List<string>();
  }
}