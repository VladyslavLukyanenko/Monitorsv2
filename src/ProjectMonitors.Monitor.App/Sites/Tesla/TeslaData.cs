using System.Text.Json.Serialization;

#nullable disable
namespace ProjectMonitors.Monitor.App.Sites.Tesla
{
  public class TeslaData
  {
    [JsonPropertyName("purchasable")] public bool Purchasable { get; set; }
    [JsonPropertyName("skuCode")] public string Error { get; set; }
    [JsonPropertyName("error")] public string SkuCode { get; set; }
  }
}