using System.Text.Json.Serialization;

namespace ProjectMonitors.SeedWork.Domain
{
  public class ProductAttribute
  {
    [JsonPropertyName("name")] public string Name { get; init; } = null!;
    [JsonPropertyName("value")] public string Value { get; init; } = null!;
  }
}