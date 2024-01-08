using System.Net.Http;

namespace ProjectMonitors.Monitor.Domain
{
  public interface IMonitorHttpClientFactory
  {
    void Configure(MonitorSettings settings);
    HttpClient CreateHttpClient();
  }
}