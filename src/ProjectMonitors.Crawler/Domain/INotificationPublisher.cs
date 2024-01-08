using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Crawler.Domain
{
  public interface INotificationPublisher
  {
    ValueTask<Result> PublishAsync(string storeId, INotificationPayloadFactory payloadFactory, CancellationToken ct);
    ValueTask<Result> PublishStatsAsync(ComponentStats stats, CancellationToken ct);
  }
}