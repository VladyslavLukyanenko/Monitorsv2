using System.Text.Json.Serialization;

namespace ProjectMonitors.Senders.Discord
{
  public class DiscordRateLimitedError
  {
    [JsonPropertyName("retry_after")] public int RetryAfter { get; set; }
  }
}