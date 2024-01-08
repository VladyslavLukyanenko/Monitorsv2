using System;
using System.Text.Json.Serialization;

namespace ProjectMonitors.SeedWork.Domain
{
  public class NotificationPayload
  {
    [JsonPropertyName("slug")] public string Slug { get; init; } = null!;
    [JsonPropertyName("targetId")] public string TargetId { get; init; } = null!;
    [JsonPropertyName("shopTitle")] public string? ShopTitle { get; init; }
    [JsonPropertyName("shopIconUrl")] public string? ShopIconUrl { get; init; }
    [JsonPropertyName("payload")] public ProductSummary Payload { get; init; } = null!;
    [JsonPropertyName("timestamp")] public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
  }
}