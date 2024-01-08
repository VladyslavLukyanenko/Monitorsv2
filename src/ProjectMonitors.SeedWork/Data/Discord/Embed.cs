using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable disable
namespace ProjectMonitors.SeedWork.Data.Discord
{
  public class Embed
  {
    [JsonPropertyName("author")] public Author Author { get; set; } = new();
    [JsonPropertyName("color")] public long Color { get; set; } = 16734450;
    [JsonPropertyName("title")] public string Title { get; set; }
    [JsonPropertyName("url")] public string Url { get; set; } = "";
    [JsonPropertyName("timestamp")] public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    [JsonPropertyName("image")] public Image Image { get; set; } = new();
    [JsonPropertyName("thumbnail")] public Image Thumbnail { get; set; } = new();

    [JsonPropertyName("footer")]
    public Footer Footer { get; set; } = new()
    {
      Text = "BandarBounties x SealedVoid"
    };

    [JsonPropertyName("fields")] public List<Field> Fields { get; set; } = new();
  }
}