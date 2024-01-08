using System;
using System.Collections.Concurrent;
using System.Net.Http;
using ProjectMonitors.SeedWork.Http;
using ProjectMonitors.Crawler.Domain;

namespace ProjectMonitors.Crawler.Infra
{
  public class ScraperHttpClientFactory : IScraperHttpClientFactory
  {
    private readonly ConcurrentBag<HttpClient> _spawnedClients = new();
    private ScraperConfig _settings;

    public ScraperHttpClientFactory(ScraperConfig config)
    {
      _settings = config;
    }

    public void Configure(ScraperConfig settings)
    {
      _settings = settings;
      foreach (var client in _spawnedClients)
      {
        client.CancelPendingRequests();
        client.Dispose();
      }

      _spawnedClients.Clear();
    }

    public HttpClient CreateClient()
    {
      var handler = new BalancingHttpClientHandler(_settings!.Proxies);

      var client = new HttpClient(handler, true)
      {
        Timeout = TimeSpan.FromSeconds(5),
        DefaultRequestHeaders =
        {
          {
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.8.431.141 Safari/537.36"
          },
          {"Accept-Encoding", "gzip, deflate, br"},
          {"Accept", "*/*"},
          {"Connection", "keep-alive"},
          {"Cache-Control", "no-cache"},
        }
      };

      _spawnedClients.Add(client);

      return client;
    }
  }
}