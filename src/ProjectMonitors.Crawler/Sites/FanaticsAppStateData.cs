using System.Text.Json.Serialization;

namespace ProjectMonitors.Crawler.Sites
{
  public class FanaticsAppStateData
  {
    [JsonPropertyName("totalProductCount")]
    public long TotalProductCount { get; set; }

    [JsonPropertyName("siteId")] public long SiteId { get; set; }
  }

  public class PageRequestParams
  {
    [JsonPropertyName("pageSize")] public string PageSize { get; set; }
  }
}