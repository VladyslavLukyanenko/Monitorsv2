﻿using System.Text.Json.Serialization;

#nullable disable
namespace ProjectMonitors.SeedWork.Data.Discord
{
  public class Author
  {
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("url")] public string Url { get; set; } = "";
    [JsonPropertyName("icon_url")] public string IconUrl { get; set; }
  }
}