using System.Net.Http;

namespace ProjectMonitors.Crawler.Domain
{
  public interface IScraperHttpClientFactory
  {
    void Configure(ScraperConfig config);
    HttpClient CreateClient();
  }
}