using System.Threading;
using System.Threading.Tasks;

namespace ProjectMonitors.Crawler.Domain
{
  public interface IStoreSettingsRepository
  {
    ValueTask<ScraperConfig?> GetSettingsAsync(ScraperInfo scraperInfo, CancellationToken ct);
  }
}