﻿using System.Text.Json.Serialization;

#nullable disable
namespace ProjectMonitors.SeedWork.Data.Discord
{
  public class Footer
  {
    [JsonPropertyName("text")] public string Text { get; set; }
    [JsonPropertyName("icon_url")] public string IconUrl { get; set; }
  }
}