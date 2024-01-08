using System.Threading;
using System.Threading.Tasks;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Crawler.Domain
{
  public interface ICrawler
  {
    ValueTask IndexAsync(CancellationToken ct = default);
    void StartWatchers(CancellationToken ct = default);
    ValueTask ClearIndexAsync(CancellationToken ct = default);

    event AsyncEventHandler<ProductsUpdatedEventArgs> ProductsUpdated;
  }
}