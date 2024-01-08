using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Logging;
using ProjectIndustries.KingHttpClient;
using ProjectMonitors.Monitor.Domain;
using ProjectMonitors.SeedWork.Http;

namespace ProjectMonitors.Monitor.App
{
  public class MonitorHttpClientFactory : IMonitorHttpClientFactory
  {
    private readonly ConcurrentBag<HttpClient> _spawnedClients = new();
    private MonitorSettings? _settings;
    private uint _clientIx;

    private int _workersCount;

    private readonly ILoggerFactory _loggerFactory;
    private readonly IAntibotProtectionSolverProvider _solverProvider;

    public MonitorHttpClientFactory(ILoggerFactory loggerFactory, IAntibotProtectionSolverProvider solverProvider)
    {
      _loggerFactory = loggerFactory;
      _solverProvider = solverProvider;
    }

    public void Configure(MonitorSettings settings)
    {
      _settings = settings;
      _clientIx = 0;
      _workersCount = settings.CalculateTotalWorkersCount();
      foreach (var client in _spawnedClients)
      {
        client.CancelPendingRequests();
        client.Dispose();
      }

      _spawnedClients.Clear();
    }

    public HttpClient CreateHttpClient()
    {
      var ix = Interlocked.Increment(ref _clientIx) - 1;
      var proxies = Array.Empty<Uri>();
      if (_settings != null)
      {
        var takeCount = (int) Math.Round(_settings.Proxies.Count / (double) _workersCount);
        proxies = _settings.Proxies.Skip((int) ix * takeCount)
          .Take(takeCount)
          .ToArray();
      }

      HttpMessageHandler handler = new BalancingHttpClientHandler(proxies, CreateHttpMessageHandler);
      if (_settings?.AntibotConfig.IsEmpty() == false)
      {
        var provider = _settings.AntibotConfig.ProtectProvider;
        var solver = _solverProvider.GetSolver(provider)
                     ?? throw new InvalidOperationException($"Antibot solver {provider} is not supported");

        handler = new ProtectionSolverHttpClientHandler(handler, solver, _settings.AntibotConfig);
      }

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
          {"Connection", "keep-alive"},
          {"sec-ch-ua", "\" Not;A Brand\";v=\"99\", \"Google Chrome\";v=\"91\", \"Chromium\";v=\"91\""},
          {"sec-ch-ua-mobile", "?0"},
          {"sec-fetch-dest", "document"},
          {"sec-fetch-mode", "navigate"},
          {"sec-fetch-site", "same-origin"},
          {"sec-fetch-user", "?1"},
          {"upgrade-insecure-requests", "1"},
          {"accept", "*/*"},
          {"accept-encoding", "gzip, deflate, br"},
          {"accept-language", "en-US,en;q=0.9,uk;q=0.8,ru;q=0.7,fr;q=0.6"},
          {"cache-control", "no-cache"},
          {
            KnownHeader.HeaderOrderHeaderName, string.Join(",", new[]
            {
              "accept",
              "accept-encoding",
              "accept-language",
              "cache-control",
              "cookie",
              "pragma",
              "sec-ch-ua",
              "sec-ch-ua-mobile",
              "sec-fetch-dest",
              "sec-fetch-mode",
              "sec-fetch-site",
              "sec-fetch-user",
              "upgrade-insecure-requests",
              "user-agent",
            })
          },
          {
            KnownHeader.PHeaderOrderHeaderName, string.Join(",", new[]
            {
              ":authority",
              ":method",
              ":path",
              ":scheme",
            })
          }
        }
      };

      _spawnedClients.Add(client);

      return client;

      HttpMessageHandler CreateHttpMessageHandler(Uri? proxyUri)
      {
        return UtlsHttpMessageHandler.Create(ClientHelloSpecPresets.CreateIos121ClientHelloSpec(),
          proxyUri?.ToString() /*,
          _loggerFactory.CreateLogger<UtlsHttpMessageHandler>()*/);
      }
    }
  }
}