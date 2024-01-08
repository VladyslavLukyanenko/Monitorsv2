using System;
using System.Text.Json.Serialization;

namespace ProjectMonitors.SeedWork.Domain
{
  public class ProductLink
  {
    [JsonPropertyName("text")] public string Text { get; init; } = null!;
    [JsonPropertyName("uri")]public Uri Url { get; init; } = null!;
  }
}