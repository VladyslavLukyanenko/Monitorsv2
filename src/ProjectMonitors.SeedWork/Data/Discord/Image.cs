using System.Text.Json.Serialization;

namespace ProjectMonitors.SeedWork.Data.Discord
{
  public class Image
  {
    [JsonPropertyName("url")] public string Url { get; set; } = "";
  }
}